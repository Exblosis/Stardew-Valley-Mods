using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LetsMoveIt.TileData
{
    internal partial class Tile
    {
        private static ModConfig Config = null!;
        private static IModHelper Helper = null!;
        private static IMonitor Monitor = null!;

        public static string? TileName;
        public static string? TileIndex;
        public static Guid? TileGuid;
        public static object? TileObject;
        public static GameLocation TileLocation = null!;
        public static Vector2 TilePosition;
        public static Vector2 TileOffset;
        private static readonly HashSet<Vector2> BoundingBoxTile = [];

        public Tile(ModConfig config, IModHelper modHelper, IMonitor monitor) {
            Config = config;
            Helper = modHelper;
            Monitor = monitor;
        }

        public static void ButtonAction(GameLocation location, Vector2 tile)
        {
            if (Config.ModKey == SButton.None)
                return;
            if (Helper.Input.IsDown(Config.ModKey))
            {
                GetTarget(location, tile, Mod1.GetGlobalMousePosition());
                return;
            }
            if (TileObject is not null)
            {
                Helper.Input.Suppress(Config.MoveKey);
                Helper.Input.Suppress(Config.OverwriteKey);
                bool overwriteTile = Helper.Input.IsDown(Config.OverwriteKey);
                if (IsOccupied(location, tile) && !overwriteTile)
                {
                    Game1.playSound("cancel");
                    return;
                }
                MoveObject(location, tile, overwriteTile);
            }
        }

        private static void CancleAction()
        {
        }

        private static bool IsOccupied(GameLocation location, Vector2 tile)
        {
            bool occupied = false;
            if (!location.isTilePassable(tile) || !location.isTileOnMap(tile) || location.isTileHoeDirt(tile) || location.isCropAtTile((int)tile.X, (int)tile.Y) || location.IsTileBlockedBy(tile, ignorePassables: CollisionMask.All))
            {
                if (TileObject is not Crop || !location.isTileHoeDirt(tile))
                {
                    occupied = true;
                }
            }
            if (BoundingBoxTile.Count is not 0)
            {
                BoundingBoxTile.ToList().ForEach(t =>
                {
                    if (!location.isTilePassable(t) || !location.isTileOnMap(t) || location.isTileHoeDirt(t) || location.isCropAtTile((int)t.X, (int)t.Y) || location.IsTileBlockedBy(t, ignorePassables: CollisionMask.All))
                    {
                        occupied = true;
                    }
                });
            }
            return occupied;
        }

        private static bool IsPresent(GameLocation location, Vector2 tile)
        {
            location.moveObject((int)TilePosition.X, (int)TilePosition.Y, (int)tile.X, (int)tile.Y, TileIndex);
            return true;
        }

        public static void PlaySound()
        {
            if (!string.IsNullOrEmpty(Config.Sound))
                Game1.playSound(Config.Sound);
        }
    }
}
