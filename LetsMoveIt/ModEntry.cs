using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace LetsMoveIt
{
    /// <summary>The mod entry point.</summary>
    internal partial class ModEntry : Mod
    {
        private static ModConfig Config = null!;

        private static object? MovingObject;
        private static GameLocation MovingLocation = null!;
        private static Vector2 MovingTile;
        private static Vector2 MovingOffset;
        private static readonly HashSet<Vector2> BoundingBoxTile = [];

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            I18n.Init(helper.Translation);

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
                MovingObject = null;
                return;
            }
            if (MovingObject is null)
                return;
            try
            {
                BoundingBoxTile.Clear();
                if (MovingObject is ResourceClump resourceClump)
                {
                    if (MovingObject is GiantCrop giantCrop)
                    {
                        var data = giantCrop.GetData();
                        //Monitor.Log("Data: " + data.TileSize, LogLevel.Debug); // <<< debug >>>
                        for (int x_offset = 0; x_offset < data.TileSize.X; x_offset++)
                        {
                            for (int y_offset = 0; y_offset < data.TileSize.Y; y_offset++)
                            {
                                e.SpriteBatch.Draw(Game1.mouseCursors, GetGridPosition(x_offset * 64, y_offset * 64), new Rectangle?(new Rectangle(194, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                            }
                        }
                        Texture2D texture = Game1.content.Load<Texture2D>(data.Texture);
                        e.SpriteBatch.Draw(texture, GetGridPosition(yOffset: -64), new Rectangle(data.TexturePosition.X, data.TexturePosition.Y, 16 * data.TileSize.X, 16 * (data.TileSize.Y + 1)), Color.White * 0.6f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                    }
                    else
                    {
                        string textureName = resourceClump.textureName.Value;
                        Texture2D texture = (textureName != null) ? Game1.content.Load<Texture2D>(textureName) : Game1.objectSpriteSheet;
                        Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(texture, resourceClump.parentSheetIndex.Value, 16, 16);
                        sourceRect.Width = resourceClump.width.Value * 16;
                        sourceRect.Height = resourceClump.height.Value * 16;
                        e.SpriteBatch.Draw(texture, GetGridPosition(), sourceRect, Color.White * 0.6f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                    }
                    var rc = resourceClump.getBoundingBox();
                    for (int x_offset = 0; x_offset < rc.Width / 64; x_offset++)
                    {
                        for (int y_offset = 0; y_offset < rc.Height / 64; y_offset++)
                        {
                            BoundingBoxTile.Add(Game1.currentCursorTile + new Vector2(x_offset, y_offset));
                        }
                    }
                }
                else if (MovingObject is TerrainFeature terrainFeature)
                {
                    var tf = terrainFeature.getBoundingBox();
                    for (int x_offset = 0; x_offset < tf.Width / 64; x_offset++)
                    {
                        BoundingBoxTile.Add(Game1.currentCursorTile + new Vector2(x_offset, 0));
                        e.SpriteBatch.Draw(Game1.mouseCursors, GetGridPosition(x_offset * 64), new Rectangle?(new Rectangle(194, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                    }
                    if (MovingObject is Bush bush)
                    {
                        Texture2D texture = Game1.content.Load<Texture2D>("TileSheets\\bushes");
                        SpriteEffects flipped = bush.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                        int tileOffset = (bush.sourceRect.Height / 16 - 1) * -64;
                        e.SpriteBatch.Draw(texture, GetGridPosition(yOffset: tileOffset), bush.sourceRect.Value, Color.White * 0.6f, 0f, Vector2.Zero, 4f, flipped, 1);
                    }
                    else if (MovingObject is Flooring flooring)
                    {
                        Texture2D texture = flooring.GetTexture();
                        Point textureCorner = flooring.GetTextureCorner();
                        e.SpriteBatch.Draw(texture, GetGridPosition(), new Rectangle(textureCorner.X, textureCorner.Y, 16, 16), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                    }
                    else if (MovingObject is HoeDirt)
                    {
                        Texture2D texture = ((Game1.currentLocation.Name.Equals("Mountain") || Game1.currentLocation.Name.Equals("Mine") || (Game1.currentLocation is MineShaft mineShaft && mineShaft.shouldShowDarkHoeDirt()) || Game1.currentLocation is VolcanoDungeon) ? Game1.content.Load<Texture2D>("TerrainFeatures\\hoeDirtDark") : Game1.content.Load<Texture2D>("TerrainFeatures\\hoeDirt"));
                        if ((Game1.currentLocation.GetSeason() == Season.Winter && !Game1.currentLocation.SeedsIgnoreSeasonsHere() && Game1.currentLocation is not MineShaft) || (Game1.currentLocation is MineShaft mineShaft2 && mineShaft2.shouldUseSnowTextureHoeDirt()))
                        {
                            texture = Game1.content.Load<Texture2D>("TerrainFeatures\\hoeDirtSnow");
                        }
                        e.SpriteBatch.Draw(texture, GetGridPosition(), new Rectangle(0, 0, 16, 16), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                    }
                    else if (MovingObject is Grass grass)
                    {
                        Texture2D texture = grass.texture.Value;
                        int grassSourceOffset = grass.grassSourceOffset.Value;
                        e.SpriteBatch.Draw(texture, GetGridPosition(yOffset: -16), new Rectangle(0, grassSourceOffset, 15, 20), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                    }
                    else if (MovingObject is FruitTree fruitTree)
                    {
                        Texture2D texture = fruitTree.texture;
                        SpriteEffects flipped = fruitTree.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                        int growthStage = fruitTree.growthStage.Value;
                        int spriteRowNumber = fruitTree.GetSpriteRowNumber();
                        int seasonIndexForLocation = Game1.GetSeasonIndexForLocation(Game1.currentLocation);
                        bool flag = fruitTree.IgnoresSeasonsHere();
                        if (fruitTree.stump.Value)
                        {
                            e.SpriteBatch.Draw(texture, GetGridPosition(-64, -64), new Rectangle(8 * 48, spriteRowNumber * 5 * 16 + 48, 48, 32), Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                        }
                        else
                        {
                            e.SpriteBatch.Draw(texture, GetGridPosition(-64, -256), new Rectangle(((flag ? 1 : seasonIndexForLocation) + System.Math.Min(growthStage, 4)) * 48, spriteRowNumber * 5 * 16, 48, 80), Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                        }
                    }
                    else if (MovingObject is Tree tree)
                    {
                        Texture2D texture = tree.texture.Value;
                        Rectangle treeTopSourceRect = new(0, 0, 48, 96);
                        Rectangle stumpSourceRect = new(32, 96, 16, 32);
                        SpriteEffects flipped = tree.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                        int growthStage = tree.growthStage.Value;
                        int seasonIndexForLocation = Game1.GetSeasonIndexForLocation(Game1.currentLocation);
                        if (tree.hasMoss.Value)
                        {
                            treeTopSourceRect.X += 96;
                            stumpSourceRect.X += 96;
                        }
                        if (tree.stump.Value)
                        {
                            e.SpriteBatch.Draw(texture, GetGridPosition(yOffset: -64), stumpSourceRect, Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                        }
                        else if (growthStage < 5)
                        {
                            Rectangle value = growthStage switch
                            {
                                0 => new Rectangle(32, 128, 16, 16),
                                1 => new Rectangle(0, 128, 16, 16),
                                2 => new Rectangle(16, 128, 16, 16),
                                _ => new Rectangle(0, 96, 16, 32),
                            };
                            e.SpriteBatch.Draw(texture, GetGridPosition(yOffset: growthStage >= 3 ? -64 : 0), value, Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                        }
                        else
                        {
                            e.SpriteBatch.Draw(texture, GetGridPosition(yOffset: -64), stumpSourceRect, Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                            e.SpriteBatch.Draw(texture, GetGridPosition(-64, -320), treeTopSourceRect, Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                        }
                    }
                }
                else if (MovingObject is Crop crop)
                {
                    crop.drawWithOffset(e.SpriteBatch, Game1.currentCursorTile, Color.White * 0.6f, 0f, new Vector2(32));
                }
                else if (MovingObject is SObject sObject)
                {
                    sObject.draw(e.SpriteBatch, (int)Game1.currentCursorTile.X * 64, (int)Game1.currentCursorTile.Y * 64 - (sObject.bigCraftable.Value ? 64 : 0), 1, 0.6f);
                }
                else if (MovingObject is Character character)
                {
                    Rectangle box = character.GetBoundingBox();
                    character.Sprite.draw(e.SpriteBatch, new Vector2(Game1.getMouseX() - 32, Game1.getMouseY() - 32) + new Vector2((character.GetSpriteWidthForPositioning() * 4 / 2), (box.Height / 2)), box.Center.Y / 10000f, 0, character.ySourceRectOffset, Color.White, false, 4f, 0f, true);
                }
                else if (MovingObject is Building building)
                {
                    float x = Game1.currentCursorTile.X - MovingOffset.X / 64;
                    float y = Game1.currentCursorTile.Y - MovingOffset.Y / 64;
                    for (int x_offset = 0; x_offset < building.tilesWide.Value; x_offset++)
                    {
                        for (int y_offset = 0; y_offset < building.tilesHigh.Value; y_offset++)
                        {
                            e.SpriteBatch.Draw(Game1.mouseCursors, new Vector2((x + x_offset) * 64 - Game1.viewport.X, (y + y_offset) * 64 - Game1.viewport.Y), new Rectangle?(new Rectangle(194, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
                        }
                    }
                }
            }
            catch { }
        }

        private static Vector2 GetGridPosition(int xOffset = 0, int yOffset = 0)
        {
            return Game1.GlobalToLocal(Game1.viewport, new Vector2(xOffset, yOffset) + new Vector2(Game1.currentCursorTile.X * 64f, Game1.currentCursorTile.Y * 64f));
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree && Game1.activeClickableMenu is not CarpenterMenu)
                return;
            if (e.Button == Config.CancelKey && MovingObject is not null)
            {
                PlaySound();
                MovingObject = null;
                Helper.Input.Suppress(e.Button);
                return;
            }
            if (e.Button == Config.MoveKey)
            {
                PickupObject(Game1.currentLocation);
            }
        }

        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            MovingObject = null;
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            MovingObject = null;
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
