using System.Drawing;
using System.Threading.Tasks;

namespace GW2_Live.GameInterface
{
    public interface IProcessHandler
    {
        void SetForeground();
        Task<Bitmap> TakeScreenshot();
        Task<Bitmap> TakeScreenshot(Rectangle region);
    }
}
