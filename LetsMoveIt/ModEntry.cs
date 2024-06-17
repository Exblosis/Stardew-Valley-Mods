using System;
using LetsMoveIt.TargetData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace LetsMoveIt
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        private static ModConfig Config = null!;

        //private static bool MultiSelect = false;
        //private static Vector2 StartCursorTile;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            I18n.Init(helper.Translation);
            Target.Init(Config, Helper, Monitor);

            if (!Config.ModEnabled)
                return;

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.Display.RenderingHud += OnRenderingHud;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            //helper.Events.Input.ButtonReleased += OnButtonReleased;
        }
        private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            if (!Config.ModEnabled)
            {
                Target.TargetObject = null;
                return;
            }
            //if (Helper.Input.IsDown(Config.ModKey) && MultiSelect)
            //{
            //    Rectangle select = new ((int)Math.Min(Game1.currentCursorTile.X, StartCursorTile.X), (int)Math.Min(Game1.currentCursorTile.Y, StartCursorTile.Y), (int)Math.Abs(Game1.currentCursorTile.X - StartCursorTile.X) + 1, (int)Math.Abs(Game1.currentCursorTile.Y - StartCursorTile.Y) + 1);
            //    for (int x_offset = 0; x_offset < select.Width; x_offset++)
            //    {
            //        for (int y_offset = 0; y_offset < select.Height; y_offset++)
            //        {
            //            e.SpriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(select.X, select.Y) * 64 + new Vector2(x_offset, y_offset) * 64), new Rectangle?(new Rectangle(194, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
            //        }
            //    }
            //}
            if (Target.TargetObject is null)
                return;
            Target.Render(e.SpriteBatch, Game1.currentLocation, Game1.currentCursorTile);
        }

        private void OnRenderingHud(object? sender, RenderingHudEventArgs e)
        {
            if (Target.TargetObject is null)
                return;
            string toolbarMessage = Config.CancelKey + " " + I18n.Message("Info.Cancel") + " | " + Config.OverwriteKey + " " + I18n.Message("Info.Force") + " | " + Config.RemoveKey + " " + I18n.Message("Info.Remove");
            Vector2 bounds = Game1.smallFont.MeasureString(toolbarMessage);
            Vector2 msgPosition = new(Game1.uiViewport.Width / 2 - bounds.X / 2, Game1.uiViewport.Height - 140);
            Utility.drawTextWithColoredShadow(e.SpriteBatch, toolbarMessage, Game1.smallFont, msgPosition, Color.White, Color.Black);
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree && Game1.activeClickableMenu is not CarpenterMenu)
                return;
            if (Config.ToggleCopyModeKey != SButton.None && e.Button == Config.ToggleCopyModeKey)
            {
                Config.CopyMode = !Config.CopyMode;
                if (Config.CopyMode)
                {
                    Game1.playSound("drumkit6");
                }
                else
                {
                    Game1.playSound("drumkit6", 200);
                }
            }
            if (e.Button == Config.CancelKey && Target.TargetObject is not null)
            {
                Target.PlaySound();
                Target.TargetObject = null;
                Helper.Input.Suppress(e.Button);
                return;
            }
            if (e.Button == Config.RemoveKey && Target.TargetObject is not null)
            {
                string select = I18n.Dialogue("Remove.Select1");
                if (Target.TargetObject is Character)
                    select = I18n.Dialogue("Remove.Select2");
                if (Target.TargetObject is Building)
                    select = I18n.Dialogue("Remove.Select3");
                Game1.player.currentLocation.createQuestionDialogue(I18n.Dialogue("Remove", new { select }), Mod1.YesNoResponses(), (Farmer f, string response) =>
                {
                    if (response == "Yes")
                    {
                        Target.Remove();
                    }
                });
                Helper.Input.Suppress(e.Button);
                return;
            }
            if (e.Button == Config.MoveKey)
            {
                //if (Helper.Input.IsDown(Config.ModKey))
                //{
                //    MultiSelect = true;
                //    StartCursorTile = Game1.currentCursorTile;
                //}
                Target.ButtonAction(Game1.currentLocation, Game1.currentCursorTile);
            }
        }

        //private void OnButtonReleased(object? sender, ButtonReleasedEventArgs e)
        //{
        //    if (e.Button == Config.MoveKey)
        //    {
        //        MultiSelect = false;
        //    }
        //}

        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if (Game1.activeClickableMenu is null)
                return;
            if (Game1.activeClickableMenu is DialogueBox)
                return;
            Target.TargetObject = null;
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            Target.TargetObject = null;
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
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => I18n.Config("RemoveKey"),
                getValue: () => Config.RemoveKey,
                setValue: value => Config.RemoveKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => I18n.Config("ToggleCopyModeKey"),
                getValue: () => Config.ToggleCopyModeKey,
                setValue: value => Config.ToggleCopyModeKey = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("CopyMode"),
                getValue: () => Config.CopyMode,
                setValue: value => Config.CopyMode = value
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
            //configMenu.AddParagraph(
            //    mod: ModManifest,
            //    text: () => I18n.Config("IndoorPot.Note")
            //);
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
