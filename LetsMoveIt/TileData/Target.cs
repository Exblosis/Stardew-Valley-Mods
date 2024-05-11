using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace LetsMoveIt.TileData
{
    internal partial class Tile
    {
        /// <summary>Get the target tile</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The current tile position.</param>
        /// <param name="map">The current map position.</param>
        public static void GetTarget(GameLocation location, Vector2 tile, Point map)
        {
            if (Config.EnableMoveEntity)
            {
                foreach (var c in location.characters)
                {
                    var bb = c.GetBoundingBox();
                    bb = new Rectangle(bb.Location - new Point(0, 64), new Point(c.Sprite.getWidth() * 4, c.Sprite.getHeight() * 4));
                    if (bb.Contains(map))
                    {
                        SetTarget(c, c.currentLocation, tile);
                        return;
                    }
                }
                if (location is Farm farm)
                {
                    foreach (var a in farm.animals.Values)
                    {
                        if (a.GetBoundingBox().Contains(map))
                        {
                            SetTarget(a, a.currentLocation, tile);
                            return;
                        }
                    }
                }
                if (location is AnimalHouse animalHouse)
                {
                    foreach (var a in animalHouse.animals.Values)
                    {
                        if (a.GetBoundingBox().Contains(map))
                        {
                            SetTarget(a, a.currentLocation, tile);
                            return;
                        }
                    }
                }
                if (location is Forest forest)
                {
                    foreach (var a in forest.marniesLivestock)
                    {
                        if (a.GetBoundingBox().Contains(map))
                        {
                            SetTarget(a, a.currentLocation, tile);
                            return;
                        }
                    }
                }
                if (Game1.player.GetBoundingBox().Contains(map))
                {
                    SetTarget(Game1.player, location, tile);
                    return;
                }
            }
            if (location.objects.TryGetValue(tile, out var obj))
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
                        SetTarget(cp, cp.currentLocation, cp.tilePosition);
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

                SetTarget(obj.Name, obj, obj.Location, obj.TileLocation);
                return;
            }
            foreach (var rc in location.resourceClumps)
            {
                if (rc.occupiesTile((int)tile.X, (int)tile.Y) && Config.EnableMoveResourceClump)
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

                    var rcGuid = location.resourceClumps.GuidOf(rc);
                    SetTarget(rcGuid, rc, rc.Location, rc.Tile);
                    return;
                }
            }
            if (location.isCropAtTile((int)tile.X, (int)tile.Y) && Config.MoveCropWithoutTile && Config.EnableMoveCrop)
            {
                var cp = ((HoeDirt)location.terrainFeatures[tile]).crop;
                SetTarget(cp, cp.currentLocation, cp.tilePosition);
                return;
            }
            if (location.largeTerrainFeatures is not null && Config.EnableMoveTerrainFeature)
            {
                foreach (var ltf in location.largeTerrainFeatures)
                {
                    if (ltf.getBoundingBox().Contains((int)tile.X * 64, (int)tile.Y * 64))
                    {
                        if ((ltf is Bush) && !Config.EnableMoveBush)
                            return;

                        SetTarget(ltf, ltf.Location, ltf.Tile);
                        return;
                    }
                }
            }
            if (location.terrainFeatures.TryGetValue(tile, out var tf) && Config.EnableMoveTerrainFeature)
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

                SetTarget(tf, tf.Location, tf.Tile);
                return;
            }
            if (location.IsTileOccupiedBy(tile, CollisionMask.Buildings) && Config.EnableMoveBuilding)
            {
                var building = location.getBuildingAt(tile);
                if (building != null)
                {
                    Vector2 buildingTile = new Vector2(building.tileX.Value, building.tileY.Value);
                    SetTarget(building, location, tile, tile - buildingTile);
                    return;
                }
            }
        }

        private static void SetTarget(object obj, GameLocation lastLocation, Vector2 cursorTile, Vector2? offset = null)
        {
            SetTarget(null, null, obj, lastLocation, cursorTile, offset ?? Vector2.Zero);
        }
        private static void SetTarget(string? name, object obj, GameLocation lastLocation, Vector2 cursorTile)
        {
            SetTarget(name, null, obj, lastLocation, cursorTile, Vector2.Zero);
        }
        private static void SetTarget(Guid guid, object obj, GameLocation lastLocation, Vector2 cursorTile)
        {
            SetTarget(null, guid, obj, lastLocation, cursorTile, Vector2.Zero);
        }
        private static void SetTarget(string? name, Guid? guid, object obj, GameLocation lastLocation, Vector2 cursorTile, Vector2 offset)
        {
            TileName = name;
            TileGuid = guid;
            TileObject = obj;
            TileLocation = lastLocation;
            TilePosition = cursorTile;
            TileOffset = offset;
            //Monitor.Log($"Picked up {obj}", LogLevel.Info); // <<< debug >>>
            Helper.Input.Suppress(Config.MoveKey);
            PlaySound();
        }
    }
}
