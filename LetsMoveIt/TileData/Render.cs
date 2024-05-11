using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace LetsMoveIt.TileData
{
    internal partial class Tile
    {
        public static void Render(RenderedWorldEventArgs e, GameLocation location, Vector2 tile)
        {
            try
            {
                BoundingBoxTile.Clear();
                if (TileObject is ResourceClump resourceClump)
                {
                    if (TileObject is GiantCrop giantCrop)
                    {
                        var data = giantCrop.GetData();
                        //Monitor.Log("Data: " + data.TileSize, LogLevel.Debug); // <<< debug >>>
                        for (int x_offset = 0; x_offset < data.TileSize.X; x_offset++)
                        {
                            for (int y_offset = 0; y_offset < data.TileSize.Y; y_offset++)
                            {
                                e.SpriteBatch.Draw(Game1.mouseCursors, Mod1.LocalCursorTile(x_offset * 64, y_offset * 64), new Rectangle?(new Rectangle(194, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                            }
                        }
                        Texture2D texture = Game1.content.Load<Texture2D>(data.Texture);
                        e.SpriteBatch.Draw(texture, Mod1.LocalCursorTile(y: -64), new Rectangle(data.TexturePosition.X, data.TexturePosition.Y, 16 * data.TileSize.X, 16 * (data.TileSize.Y + 1)), Color.White * 0.6f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                    }
                    else
                    {
                        string textureName = resourceClump.textureName.Value;
                        Texture2D texture = (textureName != null) ? Game1.content.Load<Texture2D>(textureName) : Game1.objectSpriteSheet;
                        Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(texture, resourceClump.parentSheetIndex.Value, 16, 16);
                        sourceRect.Width = resourceClump.width.Value * 16;
                        sourceRect.Height = resourceClump.height.Value * 16;
                        e.SpriteBatch.Draw(texture, Mod1.LocalCursorTile(), sourceRect, Color.White * 0.6f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                    }
                    var rc = resourceClump.getBoundingBox();
                    for (int x_offset = 0; x_offset < rc.Width / 64; x_offset++)
                    {
                        for (int y_offset = 0; y_offset < rc.Height / 64; y_offset++)
                        {
                            BoundingBoxTile.Add(tile + new Vector2(x_offset, y_offset));
                        }
                    }
                }
                else if (TileObject is TerrainFeature terrainFeature)
                {
                    var tf = terrainFeature.getBoundingBox();
                    for (int x_offset = 0; x_offset < tf.Width / 64; x_offset++)
                    {
                        BoundingBoxTile.Add(tile + new Vector2(x_offset, 0));
                        e.SpriteBatch.Draw(Game1.mouseCursors, Mod1.LocalCursorTile(x_offset * 64), new Rectangle?(new Rectangle(194, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                    }
                    if (TileObject is Bush bush)
                    {
                        Texture2D texture = Game1.content.Load<Texture2D>("TileSheets\\bushes");
                        SpriteEffects flipped = bush.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                        int tileOffset = (bush.sourceRect.Height / 16 - 1) * -64;
                        e.SpriteBatch.Draw(texture, Mod1.LocalCursorTile(y: tileOffset), bush.sourceRect.Value, Color.White * 0.6f, 0f, Vector2.Zero, 4f, flipped, 1);
                    }
                    else if (TileObject is Flooring flooring)
                    {
                        Texture2D texture = flooring.GetTexture();
                        Point textureCorner = flooring.GetTextureCorner();
                        e.SpriteBatch.Draw(texture, Mod1.LocalCursorTile(), new Rectangle(textureCorner.X, textureCorner.Y, 16, 16), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                    }
                    else if (TileObject is HoeDirt)
                    {
                        Texture2D texture = ((location.Name.Equals("Mountain") || location.Name.Equals("Mine") || (location is MineShaft mineShaft && mineShaft.shouldShowDarkHoeDirt()) || location is VolcanoDungeon) ? Game1.content.Load<Texture2D>("TerrainFeatures\\hoeDirtDark") : Game1.content.Load<Texture2D>("TerrainFeatures\\hoeDirt"));
                        if ((location.GetSeason() == Season.Winter && !location.SeedsIgnoreSeasonsHere() && location is not MineShaft) || (location is MineShaft mineShaft2 && mineShaft2.shouldUseSnowTextureHoeDirt()))
                        {
                            texture = Game1.content.Load<Texture2D>("TerrainFeatures\\hoeDirtSnow");
                        }
                        e.SpriteBatch.Draw(texture, Mod1.LocalCursorTile(), new Rectangle(0, 0, 16, 16), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                    }
                    else if (TileObject is Grass grass)
                    {
                        Texture2D texture = grass.texture.Value;
                        int grassSourceOffset = grass.grassSourceOffset.Value;
                        e.SpriteBatch.Draw(texture, Mod1.LocalCursorTile(y: -16), new Rectangle(0, grassSourceOffset, 15, 20), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                    }
                    else if (TileObject is FruitTree fruitTree)
                    {
                        Texture2D texture = fruitTree.texture;
                        SpriteEffects flipped = fruitTree.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                        int growthStage = fruitTree.growthStage.Value;
                        int spriteRowNumber = fruitTree.GetSpriteRowNumber();
                        int seasonIndexForLocation = Game1.GetSeasonIndexForLocation(location);
                        bool flag = fruitTree.IgnoresSeasonsHere();
                        if (fruitTree.stump.Value)
                        {
                            e.SpriteBatch.Draw(texture, Mod1.LocalCursorTile(-64, -64), new Rectangle(8 * 48, spriteRowNumber * 5 * 16 + 48, 48, 32), Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                        }
                        else
                        {
                            e.SpriteBatch.Draw(texture, Mod1.LocalCursorTile(-64, -256), new Rectangle(((flag ? 1 : seasonIndexForLocation) + System.Math.Min(growthStage, 4)) * 48, spriteRowNumber * 5 * 16, 48, 80), Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                        }
                    }
                    else if (TileObject is Tree tree)
                    {
                        Texture2D texture = tree.texture.Value;
                        Rectangle treeTopSourceRect = new(0, 0, 48, 96);
                        Rectangle stumpSourceRect = new(32, 96, 16, 32);
                        SpriteEffects flipped = tree.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                        int growthStage = tree.growthStage.Value;
                        int seasonIndexForLocation = Game1.GetSeasonIndexForLocation(location);
                        if (tree.hasMoss.Value)
                        {
                            treeTopSourceRect.X += 96;
                            stumpSourceRect.X += 96;
                        }
                        if (tree.stump.Value)
                        {
                            e.SpriteBatch.Draw(texture, Mod1.LocalCursorTile(y: -64), stumpSourceRect, Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
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
                            e.SpriteBatch.Draw(texture, Mod1.LocalCursorTile(y: growthStage >= 3 ? -64 : 0), value, Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                        }
                        else
                        {
                            e.SpriteBatch.Draw(texture, Mod1.LocalCursorTile(y: -64), stumpSourceRect, Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                            e.SpriteBatch.Draw(texture, Mod1.LocalCursorTile(-64, -320), treeTopSourceRect, Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                        }
                    }
                }
                else if (TileObject is Crop crop)
                {
                    crop.drawWithOffset(e.SpriteBatch, tile, Color.White * 0.6f, 0f, new Vector2(32));
                }
                else if (TileObject is SObject sObject)
                {
                    sObject.draw(e.SpriteBatch, (int)tile.X * 64, (int)tile.Y * 64 - (sObject.bigCraftable.Value ? 64 : 0), 1, 0.6f);
                }
                else if (TileObject is Character character)
                {
                    Rectangle box = character.GetBoundingBox();
                    if (character is Farmer farmer)
                    {
                        farmer.FarmerRenderer.draw(e.SpriteBatch, farmer, farmer.FarmerSprite.CurrentFrame, new Vector2(Game1.getMouseX() - 32, Game1.getMouseY() - 128), box.Center.Y / 10000f, farmer.FacingDirection == 3);
                    }
                    else
                    {
                        character.Sprite.draw(e.SpriteBatch, new Vector2(Game1.getMouseX() - 32, Game1.getMouseY() - 32) + new Vector2(character.GetSpriteWidthForPositioning() * 4 / 2, box.Height / 2), box.Center.Y / 10000f, 0, character.ySourceRectOffset, Color.White, false, 4f, 0f, true);
                    }
                }
                else if (TileObject is Building building)
                {
                    for (int x_offset = 0; x_offset < building.tilesWide.Value; x_offset++)
                    {
                        for (int y_offset = 0; y_offset < building.tilesHigh.Value; y_offset++)
                        {
                            e.SpriteBatch.Draw(Game1.mouseCursors, Mod1.LocalCursorTile(x_offset * 64, y_offset * 64) - TileOffset * 64, new Rectangle?(new Rectangle(194, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
                        }
                    }
                }
            }
            catch { }
        }

    }
}
