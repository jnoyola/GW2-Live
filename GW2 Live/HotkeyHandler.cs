using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GW2_Live
{
    class HotkeyHandler : IMessageFilter
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private IntPtr hwnd;
        private Keys? hotkey;
        private Label label;

        public HotkeyHandler(Form form, Keys? key = null, Label label = null)
        {
            hwnd = form.Handle;
            hotkey = key;

            if (label != null)
            {
                this.label = label;
                UpdateLabel();
            }
        }

        public void ListenForConfigure()
        {
            Application.AddMessageFilter(this);

            if (label != null)
            {
                label.BackColor = Color.LightGray;
            }
        }

        public void ListenForHotkey()
        {
            if (hotkey != null)
            {
                RegisterHotKey(hwnd, GetId(hotkey.Value), 0, (int)hotkey.Value);
            }
        }

        public void StopListeningForHotkey()
        {
            if (hotkey != null)
            {
                UnregisterHotKey(hwnd, GetId(hotkey.Value));
            }
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_KEYDOWN)
            {
                StopListeningForHotkey();

                hotkey = (Keys)m.WParam;
                ListenForHotkey();

                EndConfigure();

                if (label != null)
                {
                    UpdateLabel();
                }
            }

            return false;
        }

        private int GetId(Keys key)
        {
            return (int)key ^ hwnd.ToInt32();
        }

        private void UpdateLabel()
        {
            var converter = new KeysConverter();
            label.Text = converter.ConvertToString(hotkey).Replace("Oem", "").Replace("NumPad", "Num");
        }

        private void EndConfigure()
        {
            Application.RemoveMessageFilter(this);

            if (label != null)
            {
                label.BackColor = Color.Transparent;
            }
        }
    }
}
