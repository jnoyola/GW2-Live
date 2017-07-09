﻿using System;
using System.Diagnostics;
using System.Threading;
using Capture.Interface;
using Capture.Hook;
using Capture;
using EasyHook;
using System.Drawing;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace GW2_Live
{
    class ProcessHandler
    {
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        private const int ScreenshotTimeoutSeconds = 2;
        private const int ScreenshotNumRetries = 3;

        public IntPtr hwnd => gameProcess.MainWindowHandle;

        private Process gameProcess;
        private Rectangle gameRectangle;
        private CaptureProcess captureProcess;

        public ProcessHandler(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);

            if (processes.Length == 0)
            {
                throw new Exception($"No processes found matching the name {processName}");
            }

            if (processes.Length > 1)
            {
                throw new Exception($"Multiple processes found matching the name {processName}");
            }

            gameProcess = processes[0];

            Rect rect;
            GetWindowRect(hwnd, out rect);
            gameRectangle = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);

            //Config.Register("Capture", "Capture.dll");
            AttachProcess();

            captureProcess.CaptureInterface.DisplayInGameText("Testing...");
        }

        public Task<Bitmap> TakeScreenshot()
        {
            return TakeScreenshot(gameRectangle);
        }

        public Task<Bitmap> TakeScreenshot(Rectangle region)
        {
            return TakeScreenshot(region, ScreenshotNumRetries);
        }

        private Task<Bitmap> TakeScreenshot(Rectangle region, int numRetries)
        {
            var completionSource = new TaskCompletionSource<Bitmap>();

            captureProcess.CaptureInterface.BeginGetScreenshot(
                region: region,
                timeout: new TimeSpan(0, 0, ScreenshotTimeoutSeconds),
                callback: async (IAsyncResult result) =>
                {
                    using (Screenshot screenshot = captureProcess.CaptureInterface.EndGetScreenshot(result))
                    {
                        if (screenshot == null || screenshot.Data == null)
                        {
                            if (numRetries == 0)
                            {
                                throw new Exception("TakeScreenshot failed for all retries");
                            }
                            else
                            {
                                completionSource.TrySetResult(await TakeScreenshot(region, numRetries - 1));
                            }
                        }
                        else
                        {
                            captureProcess.CaptureInterface.DisplayInGameText("Screenshot captured...");
                            completionSource.TrySetResult(screenshot.ToBitmap());
                        }
                    }
                });

            return completionSource.Task;
        }

        private void AttachProcess()
        {
            // Skip if the process is already hooked (and we want to hook multiple applications)
            if (HookManager.IsHooked(gameProcess.Id))
            {
                return;
            }

            CaptureConfig captureConfig = new CaptureConfig()
            {
                Direct3DVersion = Direct3DVersion.AutoDetect
            };

            var captureInterface = new CaptureInterface();
            var s = typeof(CaptureInterface).Assembly.Location;
            captureProcess = new CaptureProcess(gameProcess, captureConfig, captureInterface);

            Thread.Sleep(10);

            if (captureProcess == null)
            {
                throw new Exception("Capture Process could not be started");
            }
        }
    }
}
