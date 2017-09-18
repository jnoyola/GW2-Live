using System.Threading.Tasks;

namespace GW2_Live.GameInterface
{
    public interface ICharacterStateProvider
    {
        Task WaitForActive();

        void SetMapRect(float mapX, float mapY, float mapWidth, float mapHeight);

        int GetUiTick();

        float GetX();
        float GetY();
        float GetZ();

        float GetPercentX();
        float GetPercentY();

        float GetVx();
        float GetVy();
        float GetVz();

        float GetAngle();

        CharacterIdentity GetIdentity();
    }
}
