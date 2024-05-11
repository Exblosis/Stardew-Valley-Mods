using LetsMoveIt.TileData;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace LetsMoveIt
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        private static ModConfig Config = null!;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            I18n.Init(helper.Translation);

            _ = new Tile(Config, Helper, Monitor);

            if (!Config.ModEnabled)
                return;

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
        }
        private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            if (!Config.ModEnabled)
            {
                Tile.TileObject = null;
                return;
            }
            if (Tile.TileObject is null)
                return;
            Tile.Render(e, Game1.currentLocation, Game1.currentCursorTile);
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree && Game1.activeClickableMenu is not CarpenterMenu)
                return;
            if (e.Button == Config.CancelKey && Tile.TileObject is not null)
            {
                Tile.PlaySound();
                Tile.TileObject = null;
                Helper.Input.Suppress(e.Button);
                return;
            }
            if (e.Button == Config.MoveKey)
            {
                Tile.ButtonAction(Game1.currentLocation, Game1.currentCursorTile);
            }
        }

        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            Tile.TileObject = null;
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            Tile.TileObject = null;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );
            // Config
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("ModEnabled"),
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => I18n.Config("ModKey"),
                getValue: () => Config.ModKey,
                setValue: value => Config.ModKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => I18n.Config("MoveKey"),
                getValue: () => Config.MoveKey,
                setValue: value => Config.MoveKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => I18n.Config("OverwriteKey"),
                tooltip: () => I18n.Config("OverwriteKey.Tooltip"),
                getValue: () => Config.OverwriteKey,
                setValue: value => Config.OverwriteKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => I18n.Config("CancelKey"),
                getValue: () => Config.CancelKey,
                setValue: value => Config.CancelKey = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => I18n.Config("Sound"),
                getValue: () => Config.Sound,
                setValue: value => Config.Sound = value
            );
            // Prioritize Crops
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => I18n.Config("PrioritizeCrops")
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("MoveCropWithoutTile"),
                getValue: () => Config.MoveCropWithoutTile,
                setValue: value => Config.MoveCropWithoutTile = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("MoveCropWithoutIndoorPot"),
                getValue: () => Config.MoveCropWithoutIndoorPot,
                setValue: value => Config.MoveCropWithoutIndoorPot = value
            );
            configMenu.AddParagraph(
                mod: ModManifest,
                text: () => I18n.Config("IndoorPot.Note")
            );
            // Enable & Disable Components Page
            configMenu.AddPageLink(
                mod: ModManifest,
                pageId: "Components",
                text: () => I18n.Config("Page.Components.Link"),
                tooltip: () => I18n.Config("Page.Components.Link.Tooltip")
            );
            configMenu.AddPage(
                mod: ModManifest,
                pageId: "Components",
                pageTitle: () => I18n.Config("Page.Components.Title")
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveBuilding"),
                getValue: () => Config.EnableMoveBuilding,
                setValue: value => Config.EnableMoveBuilding = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveEntity"),
                tooltip: () => I18n.Config("EnableMoveEntity.Tooltip"),
                getValue: () => Config.EnableMoveEntity,
                setValue: value => Config.EnableMoveEntity = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveCrop"),
                getValue: () => Config.EnableMoveCrop,
                setValue: value => Config.EnableMoveCrop = value
            );

            configMenu.AddParagraph(
                mod: ModManifest,
                text: () => "________________" // SPACE
            );
            // Objects
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveObject"),
                getValue: () => Config.EnableMoveObject,
                setValue: value => Config.EnableMoveObject = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMovePlaceableObject"),
                tooltip: () => I18n.Config("EnableMovePlaceableObject.Tooltip"),
                getValue: () => Config.EnableMovePlaceableObject,
                setValue: value => Config.EnableMovePlaceableObject = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveCollectibleObject"),
                tooltip: () => I18n.Config("EnableMoveCollectibleObject.Tooltip"),
                getValue: () => Config.EnableMoveCollectibleObject,
                setValue: value => Config.EnableMoveCollectibleObject = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveGeneratedObject"),
                tooltip: () => I18n.Config("EnableMoveGeneratedObject.Tooltip"),
                getValue: () => Config.EnableMoveGeneratedObject,
                setValue: value => Config.EnableMoveGeneratedObject = value
            );

            configMenu.AddParagraph(
                mod: ModManifest,
                text: () => "________________" // SPACE
            );
            // Resource Clumps
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveResourceClump"),
                getValue: () => Config.EnableMoveResourceClump,
                setValue: value => Config.EnableMoveResourceClump = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveGiantCrop"),
                getValue: () => Config.EnableMoveGiantCrop,
                setValue: value => Config.EnableMoveGiantCrop = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveStump"),
                getValue: () => Config.EnableMoveStump,
                setValue: value => Config.EnableMoveStump = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveHollowLog"),
                getValue: () => Config.EnableMoveHollowLog,
                setValue: value => Config.EnableMoveHollowLog = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveBoulder"),
                getValue: () => Config.EnableMoveBoulder,
                setValue: value => Config.EnableMoveBoulder = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveMeteorite"),
                getValue: () => Config.EnableMoveMeteorite,
                setValue: value => Config.EnableMoveMeteorite = value
            );

            configMenu.AddParagraph(
                mod: ModManifest,
                text: () => "________________" // SPACE
            );
            // Terrain Features
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveTerrainFeature"),
                getValue: () => Config.EnableMoveTerrainFeature,
                setValue: value => Config.EnableMoveTerrainFeature = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveFlooring"),
                getValue: () => Config.EnableMoveFlooring,
                setValue: value => Config.EnableMoveFlooring = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveTree"),
                getValue: () => Config.EnableMoveTree,
                setValue: value => Config.EnableMoveTree = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveFruitTree"),
                getValue: () => Config.EnableMoveFruitTree,
                setValue: value => Config.EnableMoveFruitTree = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveGrass"),
                getValue: () => Config.EnableMoveGrass,
                setValue: value => Config.EnableMoveGrass = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveFarmland"),
                getValue: () => Config.EnableMoveFarmland,
                setValue: value => Config.EnableMoveFarmland = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveBush"),
                getValue: () => Config.EnableMoveBush,
                setValue: value => Config.EnableMoveBush = value
            );
        }
    }
}
