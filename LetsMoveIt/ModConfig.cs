using StardewModdingAPI;

namespace LetsMoveIt
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        //public bool ProtectOverwrite { get; set; } = true;
        public bool MoveCropWithoutTile { get; set; } = true;
        public bool MoveBuilding { get; set; } = true;
        public string Sound { get; set; } = "shwip";
        public SButton ModKey { get; set; } = SButton.LeftAlt;
        public SButton MoveKey { get; set; } = SButton.MouseLeft;
        public SButton OverwriteKey { get; set; } = SButton.LeftControl;
        public SButton CancelKey { get; set; } = SButton.Escape;
    }
}
