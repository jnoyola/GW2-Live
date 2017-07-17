using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GW2_Live
{
    class MumbleHandler : IDisposable
    {
        private static readonly string MumbleLinkName = "MumbleLink";
        private const int MumbleLinkSize = 5460;
        private const float MetersPerInch = 0.0254f;

        private MemoryMappedFile file;
        private MemoryMappedViewAccessor view;

        private float mapX;
        private float mapY;
        private float mapWidth;
        private float mapHeight;

        public MumbleHandler()
        {
            file = MemoryMappedFile.CreateOrOpen(MumbleLinkName, MumbleLinkSize, MemoryMappedFileAccess.ReadWrite);
            view = file.CreateViewAccessor();
        }

        public void Dispose()
        {
            view.Dispose();
            file.Dispose();
        }

        public async Task WaitForActive(int interval = 100)
        {
            int uiTick = GetUiTick();

            while (true)
            {
                await Task.Delay(interval);

                if (GetUiTick() != uiTick)
                    return;
            }
        }

        public void SetMapRect(float mapX, float mapY, float mapWidth, float mapHeight)
        {
            this.mapX = mapX;
            this.mapY = mapY;
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
        }

        public int GetUiTick() => view.ReadInt32(4);

        public float GetX() => view.ReadSingle(8);
        public float GetY() => view.ReadSingle(16);
        public float GetZ() => view.ReadSingle(12);

        public float GetPercentX() => (GetX() / MetersPerInch - mapX) / mapWidth;
        public float GetPercentY() => 1 - (GetY() / MetersPerInch - mapY) / mapHeight;

        public float GetVx() => view.ReadSingle(20);
        public float GetVy() => view.ReadSingle(28);
        public float GetVz() => view.ReadSingle(24);

        public float GetAngle() => (float)Math.Atan2(GetVy(), GetVx());

        unsafe public CharacterIdentity GetIdentity()
        {
            byte* ptrMemMap = (byte*)0;
            view.SafeMemoryMappedViewHandle.AcquirePointer(ref ptrMemMap);
            const int offset = 8 + 36 + 512 + 36;

            byte[] arr = new byte[512];
            var json = Marshal.PtrToStringUni((IntPtr)ptrMemMap + offset);
            return JsonConvert.DeserializeObject<CharacterIdentity>(json);
        }
    }
}
