using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Linq;
using Object = StardewValley.Object;

namespace LetsMoveIt
{
    public partial class ModEntry
    {
        
        private void PickupObject(GameLocation location)
        {
            if (Config.ModKey != SButton.None && !Helper.Input.IsDown(Config.ModKey))
            {
                if (movingObject is not null)
                {
                    PlaceObject(location);
                }
                return;
            }
            Vector2 cursorTile = Game1.lastCursorTile;
            var mp = Game1.getMousePosition() + new Point(Game1.viewport.Location.X, Game1.viewport.Location.Y);

            foreach (var c in Game1.currentLocation.characters)
            {
                var bb = c.GetBoundingBox();
                if(c is NPC)
                    bb = new Rectangle(bb.Location - new Point(0, 64), new Point(64, 128));
                if (bb.Contains(mp))
                {
                    Pickup(c, cursorTile, c.currentLocation);
                    return;
                }
            }
            if (Game1.currentLocation is Farm)
            {
                foreach (var a in (Game1.currentLocation as Farm).animals.Values)
                {
                    if (a.GetBoundingBox().Contains(mp))
                    {
                        Pickup(a, cursorTile, a.currentLocation);
                        return;
                    }
                }
            }
            if (Game1.currentLocation is AnimalHouse)
            {
                foreach (var a in (Game1.currentLocation as AnimalHouse).animals.Values)
                {
                    if (a.GetBoundingBox().Contains(mp))
                    {
                        Pickup(a, cursorTile, a.currentLocation);
                        return;
                    }
                }
            }
            if (Game1.currentLocation is Forest)
            {
                foreach (var a in (Game1.currentLocation as Forest).marniesLivestock)
                {
                    if (a.GetBoundingBox().Contains(mp))
                    {
                        Pickup(a, cursorTile, a.currentLocation);
                        return;
                    }
                }
            }
            if (Game1.currentLocation.objects.TryGetValue(cursorTile, out var obj))
            {
                Pickup(obj, cursorTile, obj.Location);
                return;
            }
            if (location.IsBuildableLocation() && Config.MoveBuilding)
            {
                //SMonitor.Log("pickupBuildingM | " + Game1.getMousePosition().ToVector2() + Utility.Vector2ToPoint(Game1.currentCursorTile), LogLevel.Info); // <<< debug >>>
                var building = location.buildings.FirstOrDefault(b => b.intersects(new Rectangle(Utility.Vector2ToPoint(Game1.currentCursorTile * 64 - new Vector2(32, 32)), new Point(64, 64))));
                if (building != null)
                {
                    var mousePos = Game1.getMousePosition().ToVector2();
                    var viewport = new Vector2(Game1.viewport.X, Game1.viewport.Y);
                    var buildingPos = new Vector2(building.tileX.Value, building.tileY.Value) * 64 - viewport;
                    //SMonitor.Log("pickupBuildingP | " + buildingPos, LogLevel.Info); // <<< debug >>>
                    Pickup(building, cursorTile, mousePos - buildingPos, Game1.currentLocation);
                    return;
                }
            }
            foreach (var rc in Game1.currentLocation.resourceClumps)
            {
                if (rc.occupiesTile((int)cursorTile.X, (int)cursorTile.Y))
                {
                    //SMonitor.Log("pickup | " + Game1.currentLocation.resourceClumps.IndexOf(rc), LogLevel.Info); // <<< debug >>>
                    Pickup(rc, cursorTile, rc.Location);
                    return;
                }
            }
            if (Game1.currentLocation.isCropAtTile((int)cursorTile.X, (int)cursorTile.Y) && Config.MoveCropWithoutTile)
            {
                var cp = (Game1.currentLocation.terrainFeatures[Game1.currentCursorTile] as HoeDirt).crop;
                //SMonitor.Log("pickup | " + cp, LogLevel.Info); // <<< debug >>>
                Pickup(cp, cursorTile, cp.currentLocation);
                return;
            }
            if (Game1.currentLocation.largeTerrainFeatures is not null)
            {
                foreach (var ltf in Game1.currentLocation.largeTerrainFeatures)
                {
                    if (ltf.getBoundingBox().Contains((int)cursorTile.X * 64, (int)cursorTile.Y * 64))
                    {
                        Pickup(ltf, cursorTile, ltf.Location);
                        return;
                    }
                }
            }
            if (Game1.currentLocation.terrainFeatures.TryGetValue(cursorTile, out var tf))
            {
                Pickup(tf, cursorTile, tf.Location);
                return;

            }
        }
        public static void PlaceObject(GameLocation location) // -------------------------------- //
        {
            if (!Config.ModEnabled)
            {
                movingObject = null;
                return;
            }
            if (movingObject is null)
                return;
            SHelper.Input.Suppress(Config.MoveKey);
            if (movingObject is Character)
            {
                (movingObject as Character).Position = (Game1.getMousePosition() + new Point(Game1.viewport.Location.X - 32, Game1.viewport.Location.Y - 32)).ToVector2();
                movingObject = null;
            }
            else if (movingObject is Object)
            {
                //SMonitor.Log(Game1.currentLocation.ToString() + " | " + movingLocation, LogLevel.Info); // <<< debug >>>
                //if (Game1.currentLocation.objects.ContainsKey(movingTile)) // <-- only place in the same Map
                //{
                if (Config.ProtectOverwrite && Game1.currentLocation.objects.ContainsKey(Game1.currentCursorTile))
                {
                    Game1.playSound("cancel");
                    // SMonitor.Log($"Preventing overwrite", LogLevel.Info); // <<< debug >>>
                    return;
                }
                var obj = movingLocation.objects[movingTile];
                movingLocation.objects.Remove(movingTile);
                Game1.currentLocation.objects[Game1.currentCursorTile] = obj;
                Game1.currentLocation.objects[Game1.currentCursorTile].TileLocation = Game1.currentCursorTile;
                movingObject = null;
                //}
            }
            else if (movingObject is FarmAnimal)
            {
                (movingObject as FarmAnimal).Position = (Game1.getMousePosition() + new Point(Game1.viewport.Location.X - 32, Game1.viewport.Location.Y - 32)).ToVector2();
                movingObject = null;
            }
            else if (movingObject is ResourceClump)
            {
                var index = movingLocation.resourceClumps.IndexOf(movingObject as ResourceClump);
                if (index >= 0)
                {
                    if (Game1.currentLocation == movingLocation)
                    {
                        Game1.currentLocation.resourceClumps[index].netTile.Value = Game1.lastCursorTile;
                        movingObject = null;
                    }
                    else
                    {
                        movingLocation.resourceClumps.Remove(movingObject as ResourceClump);
                        Game1.currentLocation.resourceClumps.Add(movingObject as ResourceClump);
                        var newIndex = Game1.currentLocation.resourceClumps.IndexOf(movingObject as ResourceClump);
                        Game1.currentLocation.resourceClumps[newIndex].netTile.Value = Game1.lastCursorTile;
                        movingObject = null;
                    }
                }
            }
            else if (movingObject is TerrainFeature)
            {
                if (movingObject is LargeTerrainFeature && movingLocation.largeTerrainFeatures.Contains(movingObject as LargeTerrainFeature))
                {
                    var index = movingLocation.largeTerrainFeatures.IndexOf(movingObject as LargeTerrainFeature);
                    //SMonitor.Log("LTF: " + index, LogLevel.Info); // <<< debug >>>
                    if (index >= 0)
                    {
                        if (Game1.currentLocation == movingLocation)
                        {
                            Game1.currentLocation.largeTerrainFeatures[index].netTilePosition.Value = Game1.lastCursorTile;
                            movingObject = null;
                        }
                        else
                        {
                            movingLocation.largeTerrainFeatures.Remove(movingObject as LargeTerrainFeature);
                            Game1.currentLocation.largeTerrainFeatures.Add(movingObject as LargeTerrainFeature);
                            var newIndex = Game1.currentLocation.largeTerrainFeatures.IndexOf(movingObject as LargeTerrainFeature);
                            Game1.currentLocation.largeTerrainFeatures[newIndex].netTilePosition.Value = Game1.lastCursorTile;
                            movingObject = null;
                        }
                    }
                }
                else if (movingLocation.terrainFeatures.ContainsKey(movingTile))
                {
                    if (Config.ProtectOverwrite && Game1.currentLocation.terrainFeatures.ContainsKey(Game1.currentCursorTile))
                    {
                        Game1.playSound("cancel");
                        // SMonitor.Log($"Preventing overwrite", LogLevel.Info); // <<< debug >>>
                        return;
                    }
                    var tf = movingLocation.terrainFeatures[movingTile];
                    //SMonitor.Log("TF: " + tf, LogLevel.Info); // <<< debug >>>
                    movingLocation.terrainFeatures.Remove(movingTile);
                    Game1.currentLocation.terrainFeatures[Game1.currentCursorTile] = tf;
                    if (Game1.currentLocation.terrainFeatures[Game1.currentCursorTile] is HoeDirt)
                    {
                        (Game1.currentLocation.terrainFeatures[Game1.currentCursorTile] as HoeDirt).updateNeighbors();
                        (Game1.currentLocation.terrainFeatures[Game1.currentCursorTile] as HoeDirt).crop?.updateDrawMath(Game1.currentCursorTile);
                    }
                    movingObject = null;
                }
            }
            else if (movingObject is Crop crop)
            {
                if (Game1.currentLocation.isCropAtTile((int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y) || !Game1.currentLocation.isTileHoeDirt(Game1.currentCursorTile))
                {
                    Game1.playSound("cancel");
                    return;
                }
                if (Game1.currentLocation.isTileHoeDirt(Game1.currentCursorTile))
                {
                    (movingLocation.terrainFeatures[movingTile] as HoeDirt).crop = null;
                    (Game1.currentLocation.terrainFeatures[Game1.currentCursorTile] as HoeDirt).crop = crop;
                    (Game1.currentLocation.terrainFeatures[Game1.currentCursorTile] as HoeDirt).crop.updateDrawMath(Game1.currentCursorTile);
                    movingObject = null;
                }
            }
            else if (movingObject is Building building)
            {
                if (location.IsBuildableLocation() && location.buildings.Contains(building))
                {
                    if (location.buildStructure(building, new Vector2((int)Math.Round(Game1.currentCursorTile.X - movingOffset.X / 64), (int)Math.Round(Game1.currentCursorTile.Y - movingOffset.Y / 64)), Game1.player, false))
                    {
                        if (movingObject is ShippingBin)
                        {
                            (movingObject as ShippingBin).initLid();
                        }
                        if (movingObject is GreenhouseBuilding)
                        {
                            Game1.getFarm().greenhouseMoved.Value = true;
                        }
                        (movingObject as Building).performActionOnBuildingPlacement();
                        movingObject = null;
                    }
                    else
                    {
                        Game1.playSound("cancel");
                        return;
                    }

                }
            }
            if (movingObject is null)
            {
                PlaySound();
            }
        }

        private static void PlaySound()
        {
            if(!string.IsNullOrEmpty(Config.Sound))
                Game1.playSound(Config.Sound);
        }

        private void Pickup(object obj, Vector2 cursorTile, GameLocation lastLocation)
        {
            Pickup(obj, cursorTile, Game1.getMousePosition().ToVector2() + new Vector2(Game1.viewport.X, Game1.viewport.Y) - cursorTile * 64, lastLocation);
        }

        private void Pickup(object obj, Vector2 cursorTile, Vector2 offset, GameLocation lastLocation)
        {
            movingObject = obj;
            movingTile = cursorTile;
            movingLocation = lastLocation;
            movingOffset = offset;
            //SMonitor.Log($"Picked up {name}"); // <<< debug >>>
            Helper.Input.Suppress(Config.MoveKey);
            PlaySound();
        }
    }
}