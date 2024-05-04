using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace LetsMoveIt
{
    internal partial class ModEntry
    {
        /// <summary>Pickup Object</summary>
        /// /// <param name="location">The current location.</param>
        private void PickupObject(GameLocation location)
        {
            if (Config.ModKey != SButton.None && !Helper.Input.IsDown(Config.ModKey))
            {
                if (MovingObject is not null)
                {
                    Helper.Input.Suppress(Config.MoveKey);
                    Helper.Input.Suppress(Config.OverwriteKey);
                    bool overwriteTile = Helper.Input.IsDown(Config.OverwriteKey);
                    PlaceObject(location, overwriteTile);
                }
                return;
            }
            Vector2 cursorTile = Game1.currentCursorTile;
            var mp = Game1.getMousePosition() + new Point(Game1.viewport.Location.X, Game1.viewport.Location.Y);
            foreach (var c in location.characters)
            {
                if (Config.EnableMoveEntity)
                {
                    var bb = c.GetBoundingBox();
                    bb = new Rectangle(bb.Location - new Point(0, 64), new Point(c.Sprite.getWidth() * 4, c.Sprite.getHeight() * 4));
                    if (bb.Contains(mp))
                    {
                        Pickup(c, cursorTile, c.currentLocation);
                        return;
                    }
                }
            }
            if ((location is Farm farm) && Config.EnableMoveEntity)
            {
                foreach (var a in farm.animals.Values)
                {
                    if (a.GetBoundingBox().Contains(mp))
                    {
                        Pickup(a, cursorTile, a.currentLocation);
                        return;
                    }
                }
            }
            if ((location is AnimalHouse animalHouse) && Config.EnableMoveEntity)
            {
                foreach (var a in animalHouse.animals.Values)
                {
                    if (a.GetBoundingBox().Contains(mp))
                    {
                        Pickup(a, cursorTile, a.currentLocation);
                        return;
                    }
                }
            }
            if ((location is Forest forest) && Config.EnableMoveEntity)
            {
                foreach (var a in forest.marniesLivestock)
                {
                    if (a.GetBoundingBox().Contains(mp))
                    {
                        Pickup(a, cursorTile, a.currentLocation);
                        return;
                    }
                }
            }
            if (location.objects.TryGetValue(cursorTile, out var obj))
            {
                if ((obj is IndoorPot pot) && Config.MoveCropWithoutIndoorPot)
                {
                    //pot.NetFields.GetFields().ToList().ForEach(l =>
                    //    Monitor.Log(l.Name + ": " + l, LogLevel.Debug) // <<< List NetFields >>> <<< debug >>>
                    //);
                    //if (pot.bush.Value is not null && Config.EnableMoveBush)
                    //{
                    //    var b = (obj as IndoorPot).bush.Value;
                    //    Pickup(b, cursorTile, b.Location);
                    //    return;
                    //}
                    if (pot.hoeDirt.Value.crop is not null && Config.EnableMoveCrop)
                    {
                        var cp = pot.hoeDirt.Value.crop;
                        Pickup(cp, cursorTile, cp.currentLocation);
                        return;
                    }
                }

                if (!Config.EnableMoveObject)
                    return;
                if (obj.isPlaceable() && !Config.EnableMovePlaceableObject)
                    return;
                if (obj.IsSpawnedObject && !Config.EnableMoveCollectibleObject)
                    return;
                if (!obj.isPlaceable() && !obj.IsSpawnedObject && !Config.EnableMoveGeneratedObject)
                    return;

                Pickup(obj, cursorTile, obj.Location);
                return;
            }
            foreach (var rc in location.resourceClumps)
            {
                if (rc.occupiesTile((int)cursorTile.X, (int)cursorTile.Y) && Config.EnableMoveResourceClump)
                {
                    int rcIndex = rc.parentSheetIndex.Value;
                    if ((rc is GiantCrop) && !Config.EnableMoveGiantCrop)
                        return;
                    if ((rcIndex is ResourceClump.stumpIndex) && !Config.EnableMoveStump)
                        return;
                    if ((rcIndex is ResourceClump.hollowLogIndex) && !Config.EnableMoveHollowLog)
                        return;
                    if ((rcIndex is ResourceClump.boulderIndex or ResourceClump.quarryBoulderIndex or ResourceClump.mineRock1Index or ResourceClump.mineRock2Index or ResourceClump.mineRock3Index or ResourceClump.mineRock4Index) && !Config.EnableMoveBoulder)
                        return;
                    if ((rcIndex is ResourceClump.meteoriteIndex) && !Config.EnableMoveMeteorite)
                        return;

                    Pickup(rc, cursorTile, rc.Location);
                    return;
                }
            }
            if (location.isCropAtTile((int)cursorTile.X, (int)cursorTile.Y) && Config.MoveCropWithoutTile && Config.EnableMoveCrop)
            {
                var cp = ((HoeDirt)location.terrainFeatures[cursorTile]).crop;
                Pickup(cp, cursorTile, cp.currentLocation);
                return;
            }
            if (location.largeTerrainFeatures is not null && Config.EnableMoveTerrainFeature)
            {
                foreach (var ltf in location.largeTerrainFeatures)
                {
                    if (ltf.getBoundingBox().Contains((int)cursorTile.X * 64, (int)cursorTile.Y * 64))
                    {
                        if ((ltf is Bush) && !Config.EnableMoveBush)
                            return;

                        Pickup(ltf, cursorTile, ltf.Location);
                        return;
                    }
                }
            }
            if (location.terrainFeatures.TryGetValue(cursorTile, out var tf) && Config.EnableMoveTerrainFeature)
            {
                if ((tf is Flooring) && !Config.EnableMoveFlooring)
                    return;
                if ((tf is Tree) && !Config.EnableMoveTree)
                    return;
                if ((tf is FruitTree) && !Config.EnableMoveFruitTree)
                    return;
                if ((tf is Grass) && !Config.EnableMoveGrass)
                    return;
                if ((tf is HoeDirt) && !Config.EnableMoveFarmland)
                    return;
                if ((tf is Bush) && !Config.EnableMoveBush) // Tea Bush
                    return;

                Pickup(tf, cursorTile, tf.Location);
                return;
            }
            if (location.IsTileOccupiedBy(cursorTile, CollisionMask.Buildings) && Config.EnableMoveBuilding)
            {
                var building = location.getBuildingAt(cursorTile);
                if (building != null)
                {
                    Vector2 mousePos = GetGridPosition();
                    Vector2 viewport = new(Game1.viewport.X, Game1.viewport.Y);
                    Vector2 buildingPos = new Vector2(building.tileX.Value, building.tileY.Value) * 64 - viewport;
                    Pickup(building, cursorTile, mousePos - buildingPos, location);
                    return;
                }
            }
        }

        /// <summary>Place Object</summary>
        /// <param name="location">The current location.</param>
        /// <param name="overwriteTile">To Overwrite existing Object.</param>
        public static void PlaceObject(GameLocation location, bool overwriteTile)
        {
            if (!Config.ModEnabled)
            {
                MovingObject = null;
                return;
            }
            if (MovingObject is null)
                return;
            Vector2 cursorTile = Game1.currentCursorTile;
            if (!overwriteTile)
            {
                if (!location.isTilePassable(cursorTile) || !location.isTileOnMap(cursorTile) || location.isTileHoeDirt(cursorTile) || location.isCropAtTile((int)cursorTile.X, (int)cursorTile.Y) || location.IsTileBlockedBy(cursorTile, ignorePassables: CollisionMask.All))
                {
                    if (MovingObject is not Crop || !location.isTileHoeDirt(cursorTile))
                    {
                        Game1.playSound("cancel");
                        return;
                    }
                }
                if (BoundingBoxTile.Count is not 0)
                {
                    bool occupied = false;
                    BoundingBoxTile.ToList().ForEach(b =>
                    {
                        if (!location.isTilePassable(b) || !location.isTileOnMap(b) || location.isTileHoeDirt(b) || location.isCropAtTile((int)b.X, (int)b.Y) || location.IsTileBlockedBy(b, ignorePassables: CollisionMask.All))
                        {
                            occupied = true;
                        }
                    });
                    if (occupied)
                    {
                        Game1.playSound("cancel");
                        return;
                    }
                }
            }
            if (MovingObject is Character character)
            {
                character.Position = (Game1.getMousePosition() + new Point(Game1.viewport.Location.X - 32, Game1.viewport.Location.Y - 32)).ToVector2();
                if (character is not Monster)
                    character.Halt();
                MovingObject = null;
            }
            else if (MovingObject is FarmAnimal farmAnimal)
            {
                farmAnimal.Position = (Game1.getMousePosition() + new Point(Game1.viewport.Location.X - 32, Game1.viewport.Location.Y - 32)).ToVector2();
                MovingObject = null;
            }
            else if (MovingObject is SObject sObject)
            {
                MovingLocation.objects.Remove(MovingTile);
                if (location.objects.ContainsKey(cursorTile))
                {
                    location.objects.Remove(cursorTile);
                }
                location.objects.Add(cursorTile, sObject);
                MovingObject = null;
            }
            else if (MovingObject is ResourceClump resourceClump)
            {
                int index = MovingLocation.resourceClumps.IndexOf(resourceClump);
                if (index >= 0)
                {
                    if (location == MovingLocation)
                    {
                        location.resourceClumps[index].netTile.Value = cursorTile;
                        MovingObject = null;
                    }
                    else
                    {
                        MovingLocation.resourceClumps.Remove(resourceClump);
                        location.resourceClumps.Add(resourceClump);
                        int newIndex = location.resourceClumps.IndexOf(resourceClump);
                        location.resourceClumps[newIndex].netTile.Value = cursorTile;
                        MovingObject = null;
                    }
                }
            }
            else if (MovingObject is TerrainFeature terrainFeature)
            {
                if (MovingObject is LargeTerrainFeature largeTerrainFeature && MovingLocation.largeTerrainFeatures.Contains(largeTerrainFeature))
                {
                    int index = MovingLocation.largeTerrainFeatures.IndexOf(largeTerrainFeature);
                    if (index >= 0)
                    {
                        if (location == MovingLocation)
                        {
                            location.largeTerrainFeatures[index].netTilePosition.Value = cursorTile;
                            MovingObject = null;
                        }
                        else
                        {
                            MovingLocation.largeTerrainFeatures.Remove(largeTerrainFeature);
                            location.largeTerrainFeatures.Add(largeTerrainFeature);
                            int newIndex = location.largeTerrainFeatures.IndexOf(largeTerrainFeature);
                            location.largeTerrainFeatures[newIndex].netTilePosition.Value = cursorTile;
                            MovingObject = null;
                        }
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
                //            SMonitor.Log("IP Place", LogLevel.Debug); // <<< debug >>>
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
                //        SMonitor.Log("TF Place", LogLevel.Debug); // <<< debug >>>
                //    }
                //    if (MovingLocation.objects.TryGetValue(MovingTile, out var oldOpj))
                //    {
                //        if (oldOpj is IndoorPot pot)
                //        {
                //            pot.bush.Value = null;
                //            SMonitor.Log("IP Remove", LogLevel.Debug); // <<< debug >>>
                //        }
                //    }
                //    if (MovingLocation.terrainFeatures.ContainsKey(MovingTile))
                //    {
                //        MovingLocation.terrainFeatures.Remove(MovingTile);
                //        SMonitor.Log("TF Remove", LogLevel.Debug); // <<< debug >>>
                //    }
                //}
                else if (MovingLocation.terrainFeatures.ContainsKey(MovingTile))
                {
                    MovingLocation.terrainFeatures.Remove(MovingTile);
                    if (location.terrainFeatures.ContainsKey(cursorTile))
                    {
                        location.terrainFeatures.Remove(cursorTile);
                    }
                    location.terrainFeatures.Add(cursorTile, terrainFeature);
                    HashSet<Vector2> neighbors = [cursorTile + new Vector2(0, 1), cursorTile + new Vector2(1, 0), cursorTile + new Vector2(0, -1), cursorTile + new Vector2(-1, 0)];
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
                    if (location.terrainFeatures[cursorTile] is HoeDirt hoeDirt)
                    {
                        hoeDirt.updateNeighbors();
                        hoeDirt.crop?.updateDrawMath(cursorTile);
                    }
                    MovingObject = null;
                }
            }
            else if (MovingObject is Crop crop)
            {
                if (location.isCropAtTile((int)cursorTile.X, (int)cursorTile.Y) || !location.isTileHoeDirt(cursorTile))
                {
                    Game1.playSound("cancel");
                    return;
                }
                if (location.objects.TryGetValue(cursorTile, out var newPot))
                {
                    if (newPot is IndoorPot pot)
                    {
                        if (pot.hoeDirt.Value.crop is not null)
                        {
                            Game1.playSound("cancel");
                            return;
                        }
                        pot.hoeDirt.Value.crop = crop;
                        pot.hoeDirt.Value.crop.updateDrawMath(cursorTile);
                        MovingObject = null;
                    }
                }
                if (MovingLocation.objects.TryGetValue(MovingTile, out var oldPot))
                {
                    if (oldPot is IndoorPot pot)
                    {
                        pot.hoeDirt.Value.crop = null;
                    }
                }
                if (MovingLocation.terrainFeatures.TryGetValue(MovingTile, out var oldHoeDirt))
                {
                    if (oldHoeDirt is HoeDirt hoeDirt)
                    {
                        hoeDirt.crop = null;
                    }
                }
                if (location.terrainFeatures.TryGetValue(cursorTile, out var newHoeDirt))
                {
                    if (newHoeDirt is HoeDirt hoeDirt)
                    {
                        hoeDirt.crop = crop;
                        hoeDirt.crop.updateDrawMath(cursorTile);
                        MovingObject = null;
                    }
                }
            }
            else if (MovingObject is Building building)
            {
                if (location.IsBuildableLocation())
                {
                    if (location.buildStructure(building, new Vector2(cursorTile.X - MovingOffset.X / 64, cursorTile.Y - MovingOffset.Y / 64), Game1.player, overwriteTile))
                    {
                        if (MovingObject is ShippingBin shippingBin)
                        {
                            shippingBin.initLid();
                        }
                        if (MovingObject is GreenhouseBuilding)
                        {
                            Game1.getFarm().greenhouseMoved.Value = true;
                        }
                        building.performActionOnBuildingPlacement();
                        MovingObject = null;
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
            if (MovingObject is null)
            {
                PlaySound();
            }
        }

        private static void PlaySound()
        {
            if (!string.IsNullOrEmpty(Config.Sound))
                Game1.playSound(Config.Sound);
        }

        private void Pickup(object obj, Vector2 cursorTile, GameLocation lastLocation)
        {
            Pickup(obj, cursorTile, GetGridPosition(), lastLocation);
        }

        private void Pickup(object obj, Vector2 cursorTile, Vector2 offset, GameLocation lastLocation)
        {
            MovingObject = obj;
            MovingTile = cursorTile;
            MovingLocation = lastLocation;
            MovingOffset = offset;
            //Monitor.Log($"Picked up {obj}", LogLevel.Info); // <<< debug >>>
            Helper.Input.Suppress(Config.MoveKey);
            PlaySound();
        }
    }
}
