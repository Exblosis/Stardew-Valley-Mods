using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace LetsMoveIt.TileData
{
    internal partial class Tile
    {
        /// <summary>Place Object</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The current tile position.</param>
        /// <param name="overwriteTile">To Overwrite existing Object.</param>
        public static void MoveObject(GameLocation location, Vector2 tile, bool overwriteTile)
        {
            if (!Config.ModEnabled)
            {
                TileObject = null;
                return;
            }
            if (TileObject is null)
                return;
            
            if (TileObject is Farmer farmer)
            {
                farmer.Position = (Game1.getMousePosition() + new Point(Game1.viewport.Location.X - 32, Game1.viewport.Location.Y - 32)).ToVector2();
            }
            else if (TileObject is NPC character)
            {
                if (location == TileLocation)
                {
                    character.Position = (Game1.getMousePosition() + new Point(Game1.viewport.Location.X - 32, Game1.viewport.Location.Y - 32)).ToVector2();
                }
                else
                {
                    Game1.warpCharacter(character, location, (Game1.getMousePosition() + new Point(Game1.viewport.Location.X - 32, Game1.viewport.Location.Y - 32)).ToVector2() / 64);
                }
                if (character is not Monster)
                    character.Halt();
                TileObject = null;
            }
            else if (TileObject is FarmAnimal farmAnimal)
            {
                if (location != TileLocation)
                {
                    TileLocation.animals.Remove(farmAnimal.myID.Value);
                    location.animals.TryAdd(farmAnimal.myID.Value, farmAnimal);
                }
                farmAnimal.Position = (Game1.getMousePosition() + new Point(Game1.viewport.Location.X - 32, Game1.viewport.Location.Y - 32)).ToVector2();
                TileObject = null;
            }
            else if (TileObject is SObject sObject)
            {
                if (TileLocation.objects.ContainsKey(TilePosition))
                {
                    TileLocation.objects.Remove(TilePosition);
                    if (location.objects.ContainsKey(tile))
                    {
                        location.objects.Remove(tile);
                    }
                    location.objects.Add(tile, sObject);
                    TileObject = null;
                }
                else
                {
                    TileObject = null;
                    Game1.playSound("dwop");
                    return;
                }
            }
            else if (TileObject is ResourceClump resourceClump)
            {
                int index = TileLocation.resourceClumps.IndexOf(resourceClump);
                if (index >= 0)
                {
                    if (location == TileLocation)
                    {
                        location.resourceClumps[index].netTile.Value = tile;
                        TileObject = null;
                    }
                    else
                    {
                        TileLocation.resourceClumps.Remove(resourceClump);
                        location.resourceClumps.Add(resourceClump);
                        int newIndex = location.resourceClumps.IndexOf(resourceClump);
                        location.resourceClumps[newIndex].netTile.Value = tile;
                        TileObject = null;
                    }
                }
                else
                {
                    TileObject = null;
                    Game1.playSound("dwop");
                    return;
                }
            }
            else if (TileObject is TerrainFeature terrainFeature)
            {
                if (TileObject is LargeTerrainFeature largeTerrainFeature && TileLocation.largeTerrainFeatures.Contains(largeTerrainFeature))
                {
                    int index = TileLocation.largeTerrainFeatures.IndexOf(largeTerrainFeature);
                    if (index >= 0)
                    {
                        if (location == TileLocation)
                        {
                            location.largeTerrainFeatures[index].netTilePosition.Value = tile;
                            TileObject = null;
                        }
                        else
                        {
                            TileLocation.largeTerrainFeatures.Remove(largeTerrainFeature);
                            location.largeTerrainFeatures.Add(largeTerrainFeature);
                            int newIndex = location.largeTerrainFeatures.IndexOf(largeTerrainFeature);
                            location.largeTerrainFeatures[newIndex].netTilePosition.Value = tile;
                            TileObject = null;
                        }
                    }
                    else
                    {
                        TileObject = null;
                        Game1.playSound("dwop");
                        return;
                    }
                }
                //else if (MovingObject is Bush bush)
                //{
                //    if (location.objects.TryGetValue(cursorTile, out var newObj))
                //    {
                //        if (newObj is IndoorPot pot)
                //        {
                //            if (pot.bush.Value is not null)
                //            {
                //                Game1.playSound("cancel");
                //                return;
                //            }
                //            pot.bush.Value = bush;
                //            pot.bush.Value.netTilePosition.Value = cursorTile;
                //            MovingObject = null;
                //            Monitor.Log("IP Place", LogLevel.Debug); // <<< debug >>>
                //        }
                //    }
                //    else
                //    {
                //        if (location.terrainFeatures.ContainsKey(cursorTile))
                //        {
                //            location.terrainFeatures.Remove(cursorTile);
                //        }
                //        bush.inPot.Value = false;
                //        location.terrainFeatures.Add(cursorTile, bush);
                //        MovingObject = null;
                //        Monitor.Log("TF Place", LogLevel.Debug); // <<< debug >>>
                //    }
                //    if (MovingLocation.objects.TryGetValue(MovingTile, out var oldOpj))
                //    {
                //        if (oldOpj is IndoorPot pot)
                //        {
                //            pot.bush.Value = null;
                //            Monitor.Log("IP Remove", LogLevel.Debug); // <<< debug >>>
                //        }
                //    }
                //    if (MovingLocation.terrainFeatures.ContainsKey(MovingTile))
                //    {
                //        MovingLocation.terrainFeatures.Remove(MovingTile);
                //        Monitor.Log("TF Remove", LogLevel.Debug); // <<< debug >>>
                //    }
                //}
                else if (TileLocation.terrainFeatures.ContainsKey(TilePosition))
                {
                    TileLocation.terrainFeatures.Remove(TilePosition);
                    if (location.terrainFeatures.ContainsKey(tile))
                    {
                        location.terrainFeatures.Remove(tile);
                    }
                    location.terrainFeatures.Add(tile, terrainFeature);
                    HashSet<Vector2> neighbors = [tile + new Vector2(0, 1), tile + new Vector2(1, 0), tile + new Vector2(0, -1), tile + new Vector2(-1, 0)];
                    foreach (Vector2 ct in neighbors)
                    {
                        if (location.terrainFeatures.ContainsKey(ct))
                        {
                            if (location.terrainFeatures[ct] is HoeDirt hoeDirtNeighbors)
                            {
                                hoeDirtNeighbors.updateNeighbors();
                            }
                        }
                    }
                    if (location.terrainFeatures[tile] is HoeDirt hoeDirt)
                    {
                        hoeDirt.updateNeighbors();
                        hoeDirt.crop?.updateDrawMath(tile);
                    }
                    TileObject = null;
                }
            }
            else if (TileObject is Crop crop)
            {
                if (location.isCropAtTile((int)tile.X, (int)tile.Y) || !location.isTileHoeDirt(tile))
                {
                    Game1.playSound("cancel");
                    return;
                }
                if (location.objects.TryGetValue(tile, out var isPot))
                {
                    if (isPot is IndoorPot pot && pot.hoeDirt.Value.crop is not null)
                    {
                        Game1.playSound("cancel");
                        return;
                    }
                }
                if (TileLocation.objects.TryGetValue(TilePosition, out var oldPot))
                {
                    if (oldPot is IndoorPot pot && pot.hoeDirt.Value.crop is not null)
                    {
                        pot.hoeDirt.Value.crop = null;
                    }
                    else
                    {
                        TileObject = null;
                        Game1.playSound("dwop");
                        return;
                    }
                }
                else if (TileLocation.terrainFeatures.TryGetValue(TilePosition, out var oldHoeDirt))
                {
                    if (oldHoeDirt is HoeDirt hoeDirt && hoeDirt.crop is not null)
                    {
                        hoeDirt.crop = null;
                    }
                    else
                    {
                        TileObject = null;
                        Game1.playSound("dwop");
                        return;
                    }
                }
                else
                {
                    TileObject = null;
                    Game1.playSound("dwop");
                    return;
                }
                if (location.objects.TryGetValue(tile, out var newPot))
                {
                    if (newPot is IndoorPot pot)
                    {
                        pot.hoeDirt.Value.crop = crop;
                        pot.hoeDirt.Value.crop.updateDrawMath(tile);
                        TileObject = null;
                    }
                }
                else if (location.terrainFeatures.TryGetValue(tile, out var newHoeDirt))
                {
                    if (newHoeDirt is HoeDirt hoeDirt)
                    {
                        hoeDirt.crop = crop;
                        hoeDirt.crop.updateDrawMath(tile);
                        TileObject = null;
                    }
                }
            }
            else if (TileObject is Building building)
            {
                if (location.IsBuildableLocation())
                {
                    if (location.buildStructure(building, tile - TileOffset, Game1.player, overwriteTile))
                    {
                        if (TileObject is ShippingBin shippingBin)
                        {
                            shippingBin.initLid();
                        }
                        if (TileObject is GreenhouseBuilding)
                        {
                            Game1.getFarm().greenhouseMoved.Value = true;
                        }
                        building.performActionOnBuildingPlacement();
                        TileObject = null;
                    }
                    else
                    {
                        Game1.playSound("cancel");
                        return;
                    }
                }
                else
                {
                    Game1.playSound("cancel");
                    return;
                }
            }
            if (TileObject is null)
            {
                PlaySound();
            }
            else
            {
                TileObject = null;
                Game1.playSound("dwop");
            }
        }
    }
}
