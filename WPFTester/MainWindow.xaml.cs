using System;
using System.Windows;
using System.Windows.Threading;

using FaxUI;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Text;

namespace WPFTester {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UIWindow {
        public MainWindow() {
            InitializeComponent();

            numBox.ValueChanged += (sender, e) => {
                if(numBox.Value >= list.Count)
                    return;
                else if(numBox.Value < 0)
                    numBox.Value = list.Count - 1;

                var ico = list[(int)numBox.Value];
                icoMoon.Icon = ico;
                icoName.Text = "Name: " + Enum.GetName(typeof(MoonIcon), ico) + "\nIndex: " + (int)ico + "\nValue: " + ((int)ico).ToString("X4");
            };
            // Lel
            numBoxPbar.ValueChanged += (sender, e) => Pbar.Value = numBoxPbar.Value;

            sliderHori.ValueChanged += (sender, e) => txbVal.Text = "Hori: " + e.NewValue;
            sliderVert.ValueChanged += (sender, e) => txbVal.Text = "Vert: " + e.NewValue;

            // Capture Box
            var regex = new Regex("(ctrl|alt|shift)", RegexOptions.IgnoreCase);
            cap.KeyDown += (sender, e) => {
                var text = "";
                // Check for modifiers
                if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    text += "Ctrl + ";
                if(Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                    text += "Alt + ";
                if(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    text += "Shift + ";

                var key = GetCharFromKey(e.Key).ToString();
                // Dont match modifier keys
                if(!regex.IsMatch(key))
                    cap.Text = text + key;
            };
        }

        List<MoonIcon> list = new List<MoonIcon>((MoonIcon[])Enum.GetValues(typeof(MoonIcon)));
        int index = 1;

        private void Button_Click(object sender, RoutedEventArgs e) {
            if(index >= list.Count)
                return;
            var ico = list[index];
            index++;

            icoMoon.Icon = ico;
            icoName.Text = "Name: " + Enum.GetName(typeof(MoonIcon), ico) + "\nIndex: " + (int)ico + "\nValue: " + ((int)ico).ToString("X4");
        }

        #region Keyboard Shizz
        public enum MapType : uint {
            MAPVK_VK_TO_VSC,
            MAPVK_VSC_TO_VK,
            MAPVK_VK_TO_CHAR,
            MAPVK_VSC_TO_VK_EX,
        }

        [DllImport("user32.dll")]
        public static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] StringBuilder pwszBuff, int cchBuff, uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);
        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        public static char GetCharFromKey(Key key) {
            var ch = ' ';
            var virtualKey = (uint)KeyInterop.VirtualKeyFromKey(key);
            var keyboardState = new byte[256];

            var scanCode = MapVirtualKey(virtualKey, MapType.MAPVK_VK_TO_VSC);
            var stringBuilder = new StringBuilder(2);

            GetKeyboardState(keyboardState);

            var result = ToUnicode(virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch(result) {
                case -1:
                case 0:
                    break;
                case 1:
                default:
                    ch = stringBuilder[0];
                    break;
            }
            return ch;
        }
        #endregion
    }
}
