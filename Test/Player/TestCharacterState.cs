using System.Threading.Tasks;
using GW2_Live.GameInterface;

namespace Test.Player
{
    class TestCharacterState : ICharacterStateProvider
    {
        public float Angle { get; set; }

        public CharacterIdentity Identity { get; set; } = new CharacterIdentity();

        public int UiTick { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public float Vx { get; set; }

        public float Vy { get; set; }

        public float Vz { get; set; }

        private float mapX, mapY, mapWidth, mapHeight;

        public float GetAngle() => Angle;

        public CharacterIdentity GetIdentity() => Identity;

        public int GetUiTick() => UiTick;

        public float GetX() => X;
        public float GetY() => Y;
        public float GetZ() => Z;

        public float GetPercentX() => (GetX() - mapX) / mapWidth;
        public float GetPercentY() => 1 - (GetY() - mapY) / mapHeight;

        public float GetVx() => Vx;
        public float GetVy() => Vy;
        public float GetVz() => Vz;

        public void SetMapRect(float mapX, float mapY, float mapWidth, float mapHeight)
        {
            this.mapX = mapX;
            this.mapY = mapY;
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
        }

        public async Task WaitForActive()
        {
            await Task.CompletedTask;
        }
    }
}
