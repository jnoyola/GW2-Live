using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW2_Live
{
    class VendorWindowHandler
    {
        private const int TabBarWidth = 50;

        private ProcessHandler proc;

        private bool isMeasured;

        private Rect window;
        private int tabX;
        private int tabY;
        private int tabDy;

        public VendorWindowHandler(ProcessHandler proc)
        {
            this.proc = proc;
        }

        public async Task Open(bool hasDialog)
        {
            if (isMeasured)
            {
                await RawOpen(hasDialog);
            }
            else
            {
                // Take screenshot.
                Bitmap before = proc.TakeScreenshot().Result;

                // Open window.
                await RawOpen(hasDialog);

                // Take screenshot.
                Bitmap after = proc.TakeScreenshot().Result;

                // Process images.
                window = GraphicsUtils.FindWindow(before, after);
                var tabBar = GraphicsUtils.ToLightmap(after, window.X, window.Y, TabBarWidth, window.Height);
                tabBar = GraphicsUtils.Erode(tabBar, TabBarWidth, window.Height, 3);
                var blobs = GraphicsUtils.FindBlobs(tabBar, TabBarWidth, window.Height);

                // Validate.
                if (blobs[0].Count < 800 || blobs[0].X0 != 0 || blobs[0].Y0 != 0 || blobs.Count < 2 || blobs.Count > 15)
                {
                    var bmp = GraphicsUtils.ToBitmap(tabBar, TabBarWidth, window.Height);
                    bmp.Save($"C:\\Users\\Jonathan\\Desktop\\Error_VendorWindowTabBar.png", System.Drawing.Imaging.ImageFormat.Png);
                    throw new Exception("Vendor Window validation failed.");
                }

                if (blobs.Count > 2)
                {
                    tabX = window.X + (blobs[1].X0 + blobs[1].X1) / 2;
                    tabY = window.Y + (blobs[1].Y0 + blobs[1].Y1) / 2;
                    tabDy = (blobs[2].Y0 + blobs[2].Y1) / 2 - (blobs[1].Y0 + blobs[1].Y1) / 2;
                    isMeasured = true;
                }
            }
        }

        public async Task SelectTab(int tab)
        {
            InputHandler.SendMouseMove(tabX, tabY + tab * tabDy);
            await Task.Delay(100);
            InputHandler.SendMouseClick();
            await Task.Delay(100);
        }

        private async Task RawOpen(bool hasDialog)
        {
            InputHandler.SendKeyInteract();
            await Task.Delay(500);

            if (hasDialog)
            {
                InputHandler.SendMouseMove(700, 260);
                await Task.Delay(100);
                InputHandler.SendMouseClick();
                await Task.Delay(500);
            }
        }
    }
}
