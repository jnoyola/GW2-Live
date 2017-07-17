using System;
using System.Runtime.InteropServices;

namespace GW2_Live
{
    static class InputHandler
    {
        [DllImport("User32.dll")]
        private static extern int SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("User32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("User32.dll")]
        private static extern short VkKeyScan(ushort ch);

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646270(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        /// <summary>
        /// http://social.msdn.microsoft.com/Forums/en/csharplanguage/thread/f0e82d6e-4999-4d22-b3d3-32b25f61fb2a
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public HARDWAREINPUT Hardware;
            [FieldOffset(0)]
            public KEYBDINPUT Keyboard;
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint Msg;
            public ushort ParamL;
            public ushort ParamH;
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort Vk;
            public ushort Scan;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        /// <summary>
        /// http://social.msdn.microsoft.com/forums/en-US/netfxbcl/thread/2abc6be8-c593-4686-93d2-89785232dacd
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int X;
            public int Y;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        private const int WM_KEYDOWN = 0x0100;
        private const ushort VK_TAB = 0x09;
        private const ushort VK_RETURN = 0x0D;
        private const uint KEYEVENTF_KEYDOWN = 0;
        private const uint KEYEVENTF_KEYUP = 2;
        private const uint KEYEVENTF_UNICODE = 4;
        private const uint KEYEVENTF_SCANCODE = 8;

        public static void SendString(string inputStr)
        {
            INPUT[] inputs = new INPUT[2 * inputStr.Length];

            int i = -1;
            foreach (ushort c in inputStr)
            {
                switch(c)
                {
                    case 8:
                        inputs[++i] = CreateKeyInput(VK_TAB, 0, KEYEVENTF_KEYDOWN);
                        inputs[++i] = CreateKeyInput(VK_TAB, 0, KEYEVENTF_KEYUP);
                        break;
                    case 10:
                        inputs[++i] = CreateKeyInput(VK_RETURN, 0, KEYEVENTF_KEYDOWN);
                        inputs[++i] = CreateKeyInput(VK_RETURN, 0, KEYEVENTF_KEYUP);
                        break;
                    default:
                        ushort k = (ushort)MapVirtualKey((uint)(VkKeyScan(c) & 0xFF), 0);
                        inputs[++i] = CreateKeyInput(0, k, KEYEVENTF_SCANCODE | KEYEVENTF_KEYDOWN);
                        inputs[++i] = CreateKeyInput(0, k, KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP);
                        break;
                }
            }

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private static INPUT CreateKeyInput(ushort vk, ushort scan, uint flags)
        {
            INPUT input = new INPUT
            {
                Type = 1
            };
            input.Data.Keyboard = new KEYBDINPUT
            {
                Vk = vk,
                Scan = scan,
                Flags = flags,
                Time = 0,
                ExtraInfo = IntPtr.Zero
            };

            return input;
        }
    }
}
