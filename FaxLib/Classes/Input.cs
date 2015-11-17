using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Forms;
using System.Linq;

namespace FaxLib.Input {
    public enum ModifierKey {
        None = 0,
        Alt = 1,
        Ctrl = 2,
        Shift = 4,
        Windows = 8
    }

    [System.Diagnostics.DebuggerStepThrough]
    public class MouseInput {
        #region Properties, Fields, & Consts

        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        static MousePoint _p;

        public static bool IsLeftDown {
            get {
                return IsKeyPushedDown(Keys.Left);
            }
        }
        public static bool IsRightDown {
            get {
                return IsKeyPushedDown(Keys.Right);
            }
        }
        #endregion

        #region Interop
        [DllImport("user32.dll")]
        static extern ushort GetAsyncKeyState(int vKey);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out MousePoint lpPoint);
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [StructLayout(LayoutKind.Sequential)]
        struct MousePoint {
            public int X;
            public int Y;
            public MousePoint(int x, int y) {
                X = x;
                Y = y;
            }
        }
        #endregion

        static bool IsKeyPushedDown(Keys key) {
            return 0 != (GetAsyncKeyState((int)key) & 0x8000);
        }

        public static void LeftDown() {
            GetCursorPos(out _p);
            mouse_event(MOUSEEVENTF_LEFTDOWN, _p.X, _p.Y, 0, 0);
        }
        public static void LeftUp() {
            GetCursorPos(out _p);
            mouse_event(MOUSEEVENTF_LEFTUP, _p.X, _p.Y, 0, 0);
        }
        public static void LeftClick() {
            GetCursorPos(out _p);
            mouse_event(MOUSEEVENTF_LEFTDOWN, _p.X, _p.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, _p.X, _p.Y, 0, 0);
        }

        public static void RightClick() {
            GetCursorPos(out _p);
            mouse_event(MOUSEEVENTF_RIGHTDOWN, _p.X, _p.Y, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, _p.X, _p.Y, 0, 0);
        }
        public static void RightDown() {
            GetCursorPos(out _p);
            mouse_event(MOUSEEVENTF_RIGHTDOWN, _p.X, _p.Y, 0, 0);
        }
        public static void RightUp() {
            GetCursorPos(out _p);
            mouse_event(MOUSEEVENTF_RIGHTUP, _p.X, _p.Y, 0, 0);
        }

        public static bool SetPosition(int x, int y) {
            return SetCursorPos(x, y);
        }
    }
}

namespace FaxLib.Input.WPF {
    [System.Diagnostics.DebuggerStepThrough]
    public struct HotKey {
        #region Properties
        /// <summary>
        /// Gets a key's code number
        /// </summary>
        public int KeyCode { get; set; }
        /// <summary>
        /// Gets the name of the key
        /// </summary>
        public string KeyName { get; set; }
        /// <summary>
        /// Gets the name of the modifier(s) applied
        /// </summary>
        public string ModifierName { get; set; }
        /// <summary>
        /// Gets the parameter so send in event
        /// </summary>
        public object Parameter { get; set; }

        /// <summary>
        /// Gets the modifier applied
        /// </summary>
        public ModifierKey Modifier { get; set; }
        /// <summary>
        /// Gets the event method for this hotkey
        /// </summary>
        public Action<object> Method { get; set; }
        static int[] keyList = new int[] { 160, 162, 91, 164, 165, 92, 163, 161 };
        #endregion

        public HotKey(Key key, ModifierKey modifier, Action<object> method, object parameter = null) {
            var _key = KeyInterop.VirtualKeyFromKey(key);
            if(keyList.Any(x => x == _key))
                throw new Exception("The key '" + Enum.GetName(typeof(Key), key) + "' cant be assigned. Please use another key.");

            KeyCode = _key;
            KeyName = Enum.GetName(typeof(Key), key);
            Modifier = modifier;
            ModifierName = Enum.GetName(typeof(ModifierKeys), modifier);
            Method = method;
            Parameter = parameter;
        }
    }

    [System.Diagnostics.DebuggerStepThrough]
    public class HotKeyHandler {
        #region Fields & Properties
        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        IntPtr handle;
        /// <summary>
        /// The collection of registered hotkeys
        /// </summary>
        public List<HotKey> Collection {
            get {
                var list = new List<HotKey>();
                foreach(var pair in Hotkeys)
                    list.Add(pair.Value);
                return list;
            }
        }
        /// <summary>
        /// The ID based collection of the hotkeys
        /// ID is key multiplied by modifier * 1000
        /// </summary>
        private Dictionary<int, HotKey> Hotkeys { get; set; }
        
        /// <summary>
        /// object sender is WPF Keys enum.
        /// </summary>
        public EventHandler KeyCaptured;
        protected virtual void OnKeyCaptured(object sender) {
            if(KeyCaptured != null)
                KeyCaptured(sender, new EventArgs());
        }
        #endregion

        /// <summary>
        /// Initializes a new HotKey handler to handle all you global hotkeys in WPF
        /// </summary>
        /// <param name="window">Parent window</param>
        public HotKeyHandler(Window window) {
            handle = new WindowInteropHelper(window).Handle;
            ComponentDispatcher.ThreadPreprocessMessage += (ref MSG msg, ref bool handled) => {
                var id = msg.wParam.ToInt32();

                if(msg.message == 0x0312 && msg.hwnd == handle) {
                    var s = id.ToString();

                    // Needs to be shrinked by x * 1000
                    var key = s.Length > 4 ? id / (int.Parse(s.Substring(0, 1)) * 1000) : id;
                    OnKeyCaptured(KeyInterop.KeyFromVirtualKey(key));
                    
                    // Only handle if this key is registered
                    if(!Hotkeys.ContainsKey(id)) {
                        var _hotkey = Hotkeys[id];
                        _hotkey.Method.Invoke(_hotkey.Parameter);
                    }
                }
            };

            window.Closing += (sender, e) => UnregisterAll();
            Hotkeys = new Dictionary<int, HotKey>();
        }

        #region Methods
        /// <summary>
        /// Registers a global key and a modifier. Invokes Method after keypress. If the key already exists it wont do anything
        /// </summary>
        /// <param name="hotkey">object to be sent to the Method. Can be null.</param>
        public bool RegisterKey(HotKey hotkey) {
            int id = ((int)hotkey.Modifier * 1000) + hotkey.KeyCode;
            if(Hotkeys.ContainsKey(id))
                throw new Exception("Hotkey with the key \"" + hotkey.KeyName + "\" and Modifier \"" + hotkey.ModifierName + "\" already exists.");

            Hotkeys.Add(id, hotkey);
            return RegisterHotKey(handle, id, (int)hotkey.Modifier, hotkey.KeyCode);
        }

        /// <summary>
        /// Unregisters global key binding from register. 
        /// </summary>
        /// <param name="key">Key of hotkey.</param>
        /// <param name="mod">Modifier of hotkey.</param>
        public bool UnregisterKey(Key key, ModifierKey mod) {
            var id = ((int)mod * 1000) + KeyInterop.VirtualKeyFromKey(key);
            return UnregisterHotKey(handle, id);
        }
        /// <summary>
        /// Unregisters global key binding from register. 
        /// </summary>
        /// <param name="hotkey">Hotkey to unregister.</param>
        public bool UnregisterKey(HotKey hotkey) {
            var id = ((int)hotkey.Modifier * 1000) + hotkey.KeyCode;
            return UnregisterHotKey(handle, id);
        }

        /// <summary>
        /// Unregisters all the keys.
        /// </summary>
        public bool UnregisterAll() {
            bool state = true;
            foreach(var pair in Hotkeys) {
                var id = ((int)pair.Value.Modifier * 1000) + pair.Value.KeyCode;
                if(!UnregisterHotKey(handle, id))
                    state = false;
            }
            return state;
        }

        public bool ChangeKey(Key key, ModifierKey mod, Key newKey) {
            foreach(var hotkey in Collection) {
                if(hotkey.KeyCode == KeyInterop.VirtualKeyFromKey(key) && hotkey.Modifier == mod) {
                    UnregisterKey(hotkey);
                    RegisterKey(new HotKey(newKey, mod, hotkey.Method, hotkey.Parameter));
                    return true;
                }
            }
            return false;
        }
        public bool ChangeMod(Key key, ModifierKey mod, ModifierKey newMod) {
            foreach(var hotkey in Collection) {
                if(hotkey.KeyCode == KeyInterop.VirtualKeyFromKey(key) && hotkey.Modifier == mod) {
                    UnregisterKey(hotkey);
                    RegisterKey(new HotKey(key, mod, hotkey.Method, hotkey.Parameter));
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}

namespace FaxLib.Input.Forms {
    [System.Diagnostics.DebuggerStepThrough]
    public struct HotKey {
        #region Properties
        public string KeyName { get; set; }
        public string ModifierName { get; set; }

        public int KeyCode { get; set; }
        public object Parameter { get; set; }

        public ModifierKey Modifier { get; set; }
        public Action<object> Method { get; set; }

        static List<int> keyList = new List<int>(new int[] { 160, 162, 91, 164, 165, 92, 163, 161 });
        #endregion

        public HotKey(Keys key, ModifierKey modifier, Action<object> method, object parameter = null) {
            KeyCode = (int)key;

            if(keyList.Contains(KeyCode))
                throw new Exception("The key '" + Enum.GetName(typeof(Keys), key) + "' cant be bound");

            KeyName = Enum.GetName(typeof(Keys), key);
            Modifier = modifier;
            ModifierName = Enum.GetName(typeof(ModifierKeys), modifier);
            Method = method;
            Parameter = parameter;
        }
    }

    [System.Diagnostics.DebuggerStepThrough]
    public class HotKeyHandler {
        #region Fields
        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        IntPtr handle;
        public List<HotKey> Collection {
            get {
                List<HotKey> list = new List<HotKey>();
                foreach(KeyValuePair<int, HotKey> pair in Hotkeys) {
                    list.Add(pair.Value);
                }
                return list;
            }
        }
        private Dictionary<int, HotKey> Hotkeys;
        /// <summary>
        /// object sender is WPF Keys enum.
        /// </summary>
        public EventHandler KeyCaptured;
        protected virtual void OnKeyCaptured(object sender) {
            if(KeyCaptured != null)
                KeyCaptured(sender, new EventArgs());
        }
        #endregion

        /// <summary>
        /// Initializes a new HotKey handler to handle all you global hotkeys in Forms
        /// </summary>
        /// <param name="form">Parent form</param>
        public HotKeyHandler(Form form) {
            handle = form.Handle;
            Hotkeys = new Dictionary<int, HotKey>();
            ComponentDispatcher.ThreadPreprocessMessage += delegate (ref MSG msg, ref bool handled) {
                var id = msg.wParam.ToInt32();
                if(msg.message == 0x0312 && msg.hwnd == handle && Hotkeys.ContainsKey(id)) {
                    //Handle HotKeys here
                    var hotkey = Hotkeys[id];
                    hotkey.Method.Invoke(hotkey.Parameter);
                }
            };

            form.Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e) { UnregisterAll(); };
        }
        /// <summary>
        /// Initializes a new HotKey handler to handle all you global hotkeys
        /// </summary>
        public HotKeyHandler() {
            handle = IntPtr.Zero;
            Hotkeys = new Dictionary<int, HotKey>();
            ComponentDispatcher.ThreadPreprocessMessage += delegate (ref MSG msg, ref bool handled) {
                var id = msg.wParam.ToInt32();
                if(msg.message == 0x0312 && msg.hwnd == handle && Hotkeys.ContainsKey(id)) {
                    //Handle HotKeys here
                    var hotkey = Hotkeys[id];
                    hotkey.Method.Invoke(hotkey.Parameter);
                }
            };
        }

        #region Methods
        /// <summary>
        /// Registers a global key and a modifier. Invokes Method after keypress. If the key already exists it wont do anything
        /// </summary>
        /// <param name="hotkey">object to be sent to the Method. Can be null.</param>
        public void RegisterKey(HotKey hotkey) {
            var id = ((int)hotkey.Modifier * 1000) + hotkey.KeyCode;

            if(Hotkeys.ContainsKey(id)) throw new Exception("Hotkey with the key \"" + hotkey.KeyName + "\" and Modifier \"" + hotkey.ModifierName + "\" already exists.");
            Hotkeys.Add(id, hotkey);
            RegisterHotKey(handle, id, (int)hotkey.Modifier, hotkey.KeyCode);
        }

        /// <summary>
        /// Unregisters global key binding from register. 
        /// </summary>
        /// <param name="key">Key of hotkey.</param>
        /// <param name="mod">Modifier of hotkey.</param>
        public void UnregisterKey(Keys key, ModifierKey mod) {
            var id = ((int)mod * 1000) + (int)key;
            UnregisterHotKey(handle, id);
        }
        /// <summary>
        /// Unregisters global key binding from register. 
        /// </summary>
        /// <param name="hotkey">Hotkey to unregister.</param>
        public void UnregisterKey(HotKey hotkey) {
            var id = ((int)hotkey.Modifier * 1000) + hotkey.KeyCode;
            UnregisterHotKey(handle, id);
        }

        /// <summary>
        /// Unregisters all the keys.
        /// </summary>
        public void UnregisterAll() {
            foreach(KeyValuePair<int, HotKey> pair in Hotkeys) {
                var id = ((int)pair.Value.Modifier * 1000) + pair.Value.KeyCode;
                UnregisterHotKey(handle, id);
            }
        }
        #endregion
    }
}