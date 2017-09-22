using System.Threading.Tasks;
using GW2_Live.GameInterface;

namespace Test.Player
{
    class TestCharacterState : ICharacterStateProvider
    {
        public float Angle { get; set; }
        public float Identity { get; set; }

        public float GetAngle()
        {
            throw new System.NotImplementedException();
        }

        public CharacterIdentity GetIdentity()
        {
            throw new System.NotImplementedException();
        }

        public float GetPercentX()
        {
            throw new System.NotImplementedException();
        }

        public float GetPercentY()
        {
            throw new System.NotImplementedException();
        }

        public int GetUiTick()
        {
            throw new System.NotImplementedException();
        }

        public float GetVx()
        {
            throw new System.NotImplementedException();
        }

        public float GetVy()
        {
            throw new System.NotImplementedException();
        }

        public float GetVz()
        {
            throw new System.NotImplementedException();
        }

        public float GetX()
        {
            throw new System.NotImplementedException();
        }

        public float GetY()
        {
            throw new System.NotImplementedException();
        }

        public float GetZ()
        {
            throw new System.NotImplementedException();
        }

        public void SetMapRect(float mapX, float mapY, float mapWidth, float mapHeight)
        {
            throw new System.NotImplementedException();
        }

        public async Task WaitForActive()
        {
            await Task.CompletedTask;
        }
    }
}
