﻿using System;
using System.Drawing;
using System.Threading.Tasks;
using GW2_Live.GameInterface;

namespace GW2_Live.Player
{
    class VendorWindowHandler
    {
        private const int MeasureNumRetries = 5;
        private const int TabBarWidth = 50;
        private const int ItemOffsetFromTabX = 100;

        private IProcessHandler proc;
        private IInputHandler input;

        private bool isMeasured;

        private Rect window;
        private int tabX;
        private int tabY;
        private int tabDy;
        private int itemX;
        private int itemY;
        private int itemDy;

        public VendorWindowHandler(IProcessHandler proc, IInputHandler input)
        {
            this.proc = proc;
            this.input = input;
        }

        public async Task FullPurchase(int tab, int item, bool hasDialog = true)
        {
            await Open(hasDialog);
            await SelectTab(tab);
            await Purchase(item);
        }

        public async Task Open(bool hasDialog)
        {
            if (isMeasured)
            {
                await RawOpen(hasDialog);
            }
            else
            {
                await OpenAndMeasure(hasDialog, MeasureNumRetries);
            }
        }

        public async Task SelectTab(int tab)
        {
            if (!isMeasured)
            {
                throw new Exception("SelectTab failed because VendorWindowHandler has not been measured");
            }

            input.MoveMouse(tabX, tabY + (tab - 1) * tabDy);
            await Task.Delay(100);
            input.Click();
            await Task.Delay(100);
        }

        public async Task Purchase(int item)
        {
            if (!isMeasured)
            {
                throw new Exception("Purchase failed because VendorWindowHandler has not been measured");
            }

            input.MoveMouse(itemX, itemY + (item - 1) * itemDy);
            await Task.Delay(100);
            input.Click(count: 1); // TODO: change count to 2 to actually purchase.
            await Task.Delay(100);
        }

        private async Task OpenAndMeasure(bool hasDialog, int numRetries)
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
            if (blobs.Count < 2 || blobs.Count > 15 || blobs[0].Count < 800 || blobs[0].X0 != 0 || blobs[0].Y0 != 0)
            {
                if (numRetries == 0)
                {
                    throw new Exception("Vendor Window validation failed for all retries.");
                }
                else
                {
                    input.Escape();
                    await Task.Delay(500);
                    await OpenAndMeasure(hasDialog, numRetries - 1);
                    return;
                }
            }

            if (blobs.Count > 2)
            {
                tabX = window.X + (blobs[1].X0 + blobs[1].X1) / 2;
                tabY = window.Y + (blobs[1].Y0 + blobs[1].Y1) / 2;
                tabDy = (blobs[2].Y0 + blobs[2].Y1) / 2 - (blobs[1].Y0 + blobs[1].Y1) / 2;
                itemX = tabX + ItemOffsetFromTabX;
                itemY = tabY;
                itemDy = tabDy * 228 / 264;
                isMeasured = true;
            }
        }

        private async Task RawOpen(bool hasDialog)
        {
            input.Interact();
            await Task.Delay(500);

            if (hasDialog)
            {
                input.MoveMouse(700, 260);
                await Task.Delay(100);
                input.Click();
                await Task.Delay(500);
            }
        }
    }
}
