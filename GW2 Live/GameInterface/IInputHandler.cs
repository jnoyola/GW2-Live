namespace GW2_Live.GameInterface
{
    interface IInputHandler
    {
        void MoveMouse(int x, int y);
        void Click(uint button, uint count);
        void Scroll(int delta);

        void MoveForward(bool toggle);
        void MoveBackward(bool toggle);
        void TurnLeft(bool toggle);
        void TurnRight(bool toggle);

        void Interact();
        void Escape();

        void Type(string input);
    }
}
