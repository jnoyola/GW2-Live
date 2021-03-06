﻿using System;
using System.Runtime.InteropServices;

namespace GW2_Live.GameInterface
{
    partial class InputHandler : IInputHandler
    {
        private readonly Keybindings keybindings;

        private int movingDirection = 0;
        private int turningDirection = 0;

        public InputHandler(Keybindings keybindings)
        {
            this.keybindings = keybindings;
        }

        public void MoveMouse(int x, int y)
        {
            SendMouseMove(x, y);
        }

        public void Click(uint button = 0, uint count = 1)
        {
            SendMouseClick(button, count);
        }

        public void Scroll(int delta)
        {
            SendMouseScroll(delta);
        }

        public void Move(int direction)
        {
            if (Math.Sign(direction) != Math.Sign(movingDirection))
            {
                // Stop moving previous direction.
                if (movingDirection > 0)
                {
                    SendKeyUp(keybindings.MoveForward);
                }
                else if (movingDirection < 0)
                {
                    SendKeyUp(keybindings.MoveBackward);
                }

                // Start moving new direction.
                if (direction > 0)
                {
                    SendKeyDown(keybindings.MoveForward);
                }
                else if (direction < 0)
                {
                    SendKeyDown(keybindings.MoveBackward);
                }

                movingDirection = direction;
            }
        }

        public void Turn(int direction)
        {
            if (Math.Sign(direction) != Math.Sign(turningDirection))
            {
                // Stop turning previous direction.
                if (turningDirection > 0)
                {
                    SendKeyUp(keybindings.TurnLeft);
                }
                else if (turningDirection < 0)
                {
                    SendKeyUp(keybindings.TurnRight);
                }

                // Start turning new direction.
                if (direction > 0)
                {
                    SendKeyDown(keybindings.TurnLeft);
                }
                else if (direction < 0)
                {
                    SendKeyDown(keybindings.TurnRight);
                }

                turningDirection = direction;
            }
        }

        public void Interact()
        {
            SendKeyPress(keybindings.Interact);
        }

        public void Escape()
        {
            SendKeyPress(keybindings.Escape);
        }

        public void Type(string input)
        {
            SendString(input);
        }
    }

    partial class InputHandler : IInputHandler
    {
        [DllImport("User32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

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
            public int MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const int WHEEL_DELTA = 120;
        private const uint KEYEVENTF_KEYDOWN = 0;
        private const uint KEYEVENTF_KEYUP = 2;
        private const uint KEYEVENTF_UNICODE = 4;
        private const uint KEYEVENTF_SCANCODE = 8;
        private const int WM_KEYDOWN = 0x0100;

        private static readonly float ScaleX = 65536f / GetSystemMetrics(SM_CXSCREEN);
        private static readonly float ScaleY = 65536f / GetSystemMetrics(SM_CYSCREEN);

        private static void SendMouseMove(int x, int y)
        {
            SendInput(
                1,
                new INPUT[]
                {
                    CreateMouseInput((int)(x * ScaleX), (int)(y * ScaleY), 0, MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE)
                },
                Marshal.SizeOf(typeof(INPUT)));
        }

        private static void SendMouseClick(uint button = 0, uint count = 1)
        {
            switch (button)
            {
                case 1:
                    button = MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP;
                    break;
                case 2:
                    button = MOUSEEVENTF_MIDDLEDOWN | MOUSEEVENTF_MIDDLEUP;
                    break;
                default:
                    button = MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP;
                    break;
            }

            INPUT input = CreateMouseInput(0, 0, 0, button);

            INPUT[] inputs = new INPUT[count];
            for (int i = 0; i < count; ++i)
            {
                inputs[i] = input;
            }

            SendInput(count, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void SendMouseScroll(int delta)
        {
            SendInput(
                1,
                new INPUT[]
                {
                    CreateMouseInput(0, 0, delta * WHEEL_DELTA, MOUSEEVENTF_WHEEL)
                },
                Marshal.SizeOf(typeof(INPUT)));
        }

        private static void SendKeyPress(ushort scanCode)
        {
            // This method uses key scan codes.
            // https://msdn.microsoft.com/en-us/library/aa299374(v=vs.60).aspx

            SendInput(
                2,
                new INPUT[]
                {
                    CreateKeyInput(0, scanCode, KEYEVENTF_SCANCODE | KEYEVENTF_KEYDOWN),
                    CreateKeyInput(0, scanCode, KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP)
                },
                Marshal.SizeOf(typeof(INPUT)));
        }

        private static void SendKeyDown(ushort scanCode)
        {
            // This method uses key scan codes.
            // https://msdn.microsoft.com/en-us/library/aa299374(v=vs.60).aspx

            SendInput(
                1,
                new INPUT[]
                {
                    CreateKeyInput(0, scanCode, KEYEVENTF_SCANCODE | KEYEVENTF_KEYDOWN)
                },
                Marshal.SizeOf(typeof(INPUT)));
        }

        private static void SendKeyUp(ushort scanCode)
        {
            // This method uses key scan codes.
            // https://msdn.microsoft.com/en-us/library/aa299374(v=vs.60).aspx

            SendInput(
                1,
                new INPUT[]
                {
                    CreateKeyInput(0, scanCode, KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP)
                },
                Marshal.SizeOf(typeof(INPUT)));
        }

        private static void SendString(string input)
        {
            INPUT[] inputs = new INPUT[2 * input.Length];

            int i = -1;
            foreach (ushort c in input)
            {
                // NOTE: this doesn't work for special characters (e.g. \t, \n) or key combinations (e.g. Shift + 5 => %).
                ushort k = (ushort)MapVirtualKey((uint)(VkKeyScan(c) & 0xFF), 0);
                inputs[++i] = CreateKeyInput(0, k, KEYEVENTF_SCANCODE | KEYEVENTF_KEYDOWN);
                inputs[++i] = CreateKeyInput(0, k, KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP);
            }

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private static INPUT CreateMouseInput(int x, int y, int mouseData, uint flags)
        {
            INPUT input = new INPUT
            {
                Type = 0
            };
            input.Data.Mouse = new MOUSEINPUT
            {
                X = x,
                Y = y,
                MouseData = mouseData,
                Flags = flags,
                Time = 0,
                ExtraInfo = IntPtr.Zero
            };

            return input;
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
