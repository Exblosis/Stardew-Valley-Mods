using Microsoft.Xna.Framework;
using StardewValley;

namespace LetsMoveIt
{
    internal class Mod1
    {
        /// <summary>Get the local cursor tile with local offset.</summary>
        /// <param name="x">Offset X</param>
        /// <param name="y">Offset Y</param>
        public static Vector2 LocalCursorTile(int x = 0, int y = 0)
        {
            return Game1.GlobalToLocal(new Vector2(x, y) + Game1.currentCursorTile * 64);
        }
        /// <summary>Get the local cursor tile with local offset.</summary>
        /// <param name="offset">Offset</param>
        public static Vector2 LocalCursorTile(Vector2 offset)
        {
            return Game1.GlobalToLocal(offset + Game1.currentCursorTile * 64);
        }

        public static Point GetGlobalMousePosition()
        {
            return Game1.getMousePosition() + new Point(Game1.viewport.X, Game1.viewport.Y);
        }
    }
}
