namespace GW2_Live.GameInterface
{
    class Keybindings
    {
        // These keybindings use key scan codes.
        // https://msdn.microsoft.com/en-us/library/aa299374(v=vs.60).aspx

        public ushort MoveForward { get; set; } = 17;
        public ushort MoveBackward { get; set; } = 31;
        public ushort TurnLeft { get; set; } = 30;
        public ushort TurnRight { get; set; } = 32;

        public ushort Interact { get; set; } = 33;
        public ushort Escape { get; set; } = 1;
    }
}
