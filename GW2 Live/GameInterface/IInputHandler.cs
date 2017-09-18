namespace GW2_Live.GameInterface
{
    public interface IInputHandler
    {
        void MoveMouse(int x, int y);
        void Click(uint button = 0, uint count = 1);
        void Scroll(int delta);

        void Move(int direction);
        void Turn(int direction);

        void Interact();
        void Escape();

        void Type(string input);
    }
}
