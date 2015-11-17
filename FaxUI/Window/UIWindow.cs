using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace FaxUI {
    [System.Diagnostics.DebuggerStepThrough]
    public class UIWindow : Window {
        double _restoreTop;
        Button _minimizeButton;
        Button _maximizeButton;
        Button _closeButton;
        Point _cursorOffset;
        TextBlock _windowStatus;
        DockPanel _windowContent;

        #region Properties
        public static readonly DependencyProperty ShadowEffectProperty = DependencyProperty.Register("ShadowEffect", typeof(Effect), typeof(UIWindow), new PropertyMetadata(null));
        public Effect ShadowEffect {
            get { return (Effect)GetValue(ShadowEffectProperty); }
            set { SetValue(ShadowEffectProperty, value); }
        }

        public static readonly DependencyProperty DetailBrushProperty = DependencyProperty.Register("DetailBrush", typeof(Brush), typeof(UIWindow), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 122, 204))));
        public Brush DetailBrush {
            get { return (Brush)GetValue(DetailBrushProperty); }
            set { SetValue(DetailBrushProperty, value); }
        }

        public Brush WindowForeground {
            get { return (Brush)GetValue(WindowForegroundProperty); }
            set { SetValue(WindowForegroundProperty, value); }
        }
        public static readonly DependencyProperty WindowForegroundProperty = DependencyProperty.Register("WindowForeground", typeof(Brush), typeof(UIWindow), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(200, 200, 200)))); //new BrushConverter().ConvertFromString("#FF2D2D30")

        public Brush InnerBackground {
            get { return (Brush)GetValue(InnerBackgroundProperty); }
            set { SetValue(InnerBackgroundProperty, value); }
        }
        public static readonly DependencyProperty InnerBackgroundProperty = DependencyProperty.Register("InnerBackground", typeof(Brush), typeof(UIWindow), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(45, 45, 48)))); //new BrushConverter().ConvertFromString("#FFC8C8C8")

        public string Status {
            get {
                return _windowStatus.Text.Substring(8, _windowStatus.Text.Length - 8);
            }
            set {
                _windowStatus.Text = "Status: " + value;
            }
        }
        public Visibility StatusVisibility {
            get {
                return _windowStatus == null ? Visibility.Visible : _windowStatus.Visibility;
            }
            set {
                _windowStatus.Visibility = value;
                _windowContent.Margin = new Thickness(10, 26, 10, (value != Visibility.Visible ? 5 : 25));
            }
        }

        public string AssemblyPath {
            get { return (string)GetValue(AssemblyPathProperty); }
            set { SetValue(AssemblyPathProperty, value); }
        }
        public static readonly DependencyProperty AssemblyPathProperty = DependencyProperty.Register("AssemblyPath", typeof(string), typeof(UIWindow), new PropertyMetadata(null));
        #endregion

        protected UIWindow() {
            SourceInitialized += (sender, e) => {
                var _handle = new WindowInteropHelper(this).Handle;
                var _hwndSource = HwndSource.FromHwnd(_handle);
                if(_hwndSource != null)
                    _hwndSource.AddHook(WndProc);
            };
            Style = (Style)FindResource("WindowStyle");

            //Loaded += (sender, e) => LoadAssemblies(AssemblyPath);

            var str = string.Format("Vs: {0} x {1}", SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
            MessageBox.Show(str);
        }

        #region Interop
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int GetDllDirectory(int bufsize, System.Text.StringBuilder buf);
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr LoadLibrary(string librayName);
        [DllImport("mylibrary")]
        static extern void InitMyLibrary();

        static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam) {
            var mmi = (Minmaxinfo)Marshal.PtrToStructure(lParam, typeof(Minmaxinfo));

            const int monitorDefaulttonearest = 0x00000002;
            var monitor = NativeMethods.MonitorFromWindow(hwnd, monitorDefaulttonearest);

            if(monitor != IntPtr.Zero) {
                var monitorInfo = new Monitorinfo();
                NativeMethods.GetMonitorInfo(monitor, monitorInfo);

                var rcWorkArea = monitorInfo.rcWork;
                var rcMonitorArea = monitorInfo.rcMonitor;

                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }
        static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            if(msg == 0x0024) {
                WmGetMinMaxInfo(hwnd, lParam);
                handled = true;
            }
            return IntPtr.Zero;
        }
        #endregion

        public static void LoadAssemblies(string path) {
            if(path == null || !Directory.Exists(path)) return;
            // List<Assembly> allAssemblies = new List<Assembly>();
            // string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach(string dll in Directory.GetFiles(path, "*.dll")) {
                // try { allAssemblies.Add(Assembly.LoadFile(dll)); }
                try {
                    Assembly.LoadFrom(dll);
                }
                // DLL is unmanaged or wrong bit version
                catch(BadImageFormatException) {
                    // var currentPath = new System.Text.StringBuilder(255);
                    // GetDllDirectory(currentPath.Length, currentPath);

                    // SetDllDirectory(path);
                    LoadLibrary(dll);
                    // SetDllDirectory(currentPath);
                }
                catch { }
            }
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            _windowStatus = (TextBlock)GetTemplateChild("WindowStatus");
            _windowContent = (DockPanel)GetTemplateChild("WindowContent");
            RegisterBorders();
            RegisterInnerBorder();
            RegisterButtons();

            SizeChanged += (sender, e) => _maximizeButton.Content = WindowState == WindowState.Normal ? (Image)FindResource("RestoreImage") : (Image)FindResource("MaximizeImage");
        }

        #region Registers
        void RegisterButtons() {
            // Register all the buttons and their events
            _closeButton = (Button)GetTemplateChild("WindowCloseButton");
            _closeButton.Click += (sender, e) => {
                OnClosing(new System.ComponentModel.CancelEventArgs());
                OnClosed(EventArgs.Empty);
                Environment.Exit(Environment.ExitCode);
            };

            _minimizeButton = (Button)GetTemplateChild("WindowMinimizeButton");
            _minimizeButton.Click += (sender, e) => WindowState = WindowState.Minimized;

            _maximizeButton = (Button)GetTemplateChild("WindowMaximizeButton");
            _maximizeButton.Click += (sender, e) => WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }
        /// <summary>
        /// Registers the events for a draggable (outer) border
        /// </summary>
        /// <param name="borderEdge"></param>
        /// <param name="border"></param>
        void RegisterBorderEvents(WindowBorderEdge borderEdge, Border border) {
            border.MouseEnter += (sender, e) => {
                if(WindowState != WindowState.Maximized && ResizeMode == ResizeMode.CanResize) {
                    switch(borderEdge) {
                        case WindowBorderEdge.Left:
                        case WindowBorderEdge.Right:
                            border.Cursor = Cursors.SizeWE;
                            break;
                        case WindowBorderEdge.Top:
                        case WindowBorderEdge.Bottom:
                            border.Cursor = Cursors.SizeNS;
                            break;
                        case WindowBorderEdge.TopLeft:
                        case WindowBorderEdge.BottomRight:
                            border.Cursor = Cursors.SizeNWSE;
                            break;
                        case WindowBorderEdge.TopRight:
                        case WindowBorderEdge.BottomLeft:
                            border.Cursor = Cursors.SizeNESW;
                            break;
                    }
                }
                else
                    border.Cursor = Cursors.Arrow;
            };

            border.MouseLeftButtonDown += (sender, e) => {
                if(WindowState != WindowState.Maximized && ResizeMode == ResizeMode.CanResize) {
                    var cursorLocation = e.GetPosition(this);
                    var cursorOffset = new Point();

                    switch(borderEdge) {
                        case WindowBorderEdge.Left:
                            cursorOffset.X = cursorLocation.X;
                            break;
                        case WindowBorderEdge.TopLeft:
                            cursorOffset.X = cursorLocation.X;
                            cursorOffset.Y = cursorLocation.Y;
                            break;
                        case WindowBorderEdge.Top:
                            cursorOffset.Y = cursorLocation.Y;
                            break;
                        case WindowBorderEdge.TopRight:
                            cursorOffset.X = (Width - cursorLocation.X);
                            cursorOffset.Y = cursorLocation.Y;
                            break;
                        case WindowBorderEdge.Right:
                            cursorOffset.X = (Width - cursorLocation.X);
                            break;
                        case WindowBorderEdge.BottomRight:
                            cursorOffset.X = (Width - cursorLocation.X);
                            cursorOffset.Y = (Height - cursorLocation.Y);
                            break;
                        case WindowBorderEdge.Bottom:
                            cursorOffset.Y = (Height - cursorLocation.Y);
                            break;
                        case WindowBorderEdge.BottomLeft:
                            cursorOffset.X = cursorLocation.X;
                            cursorOffset.Y = (Height - cursorLocation.Y);
                            break;
                    }

                    _cursorOffset = cursorOffset;
                    border.CaptureMouse();
                }
            };

            border.MouseMove += (sender, e) => {
                if(WindowState == WindowState.Maximized || !border.IsMouseCaptured || ResizeMode != ResizeMode.CanResize)
                    return;

                var cursorLocation = e.GetPosition(this);

                var nHorizontalChange = (cursorLocation.X - _cursorOffset.X);
                var pHorizontalChange = (cursorLocation.X + _cursorOffset.X);
                var nVerticalChange = (cursorLocation.Y - _cursorOffset.Y);
                var pVerticalChange = (cursorLocation.Y + _cursorOffset.Y);

                switch(borderEdge) {
                    case WindowBorderEdge.Left:
                        if(Width - nHorizontalChange <= MinWidth)
                            break;
                        Left += nHorizontalChange;
                        Width -= nHorizontalChange;
                        break;
                    case WindowBorderEdge.TopLeft:
                        if(Width - nHorizontalChange <= MinWidth)
                            break;
                        Left += nHorizontalChange;
                        Width -= nHorizontalChange;
                        if(Height - nVerticalChange <= MinHeight)
                            break;
                        Top += nVerticalChange;
                        Height -= nVerticalChange;
                        break;
                    case WindowBorderEdge.Top:
                        if(Height - nVerticalChange <= MinHeight)
                            break;
                        Top += nVerticalChange;
                        Height -= nVerticalChange;
                        break;
                    case WindowBorderEdge.TopRight:
                        if(pHorizontalChange <= MinWidth)
                            break;
                        Width = pHorizontalChange;
                        if(Height - nVerticalChange <= MinHeight)
                            break;
                        Top += nVerticalChange;
                        Height -= nVerticalChange;
                        break;
                    case WindowBorderEdge.Right:
                        if(pHorizontalChange <= MinWidth)
                            break;
                        Width = pHorizontalChange;
                        break;
                    case WindowBorderEdge.BottomRight:
                        if(pHorizontalChange <= MinWidth)
                            break;
                        Width = pHorizontalChange;
                        if(pVerticalChange <= MinHeight)
                            break;
                        Height = pVerticalChange;
                        break;
                    case WindowBorderEdge.Bottom:
                        if(pVerticalChange <= MinHeight)
                            break;
                        Height = pVerticalChange;
                        break;
                    case WindowBorderEdge.BottomLeft:
                        if(Width - nHorizontalChange <= MinWidth)
                            break;
                        Left += nHorizontalChange;
                        Width -= nHorizontalChange;
                        if(pVerticalChange <= MinHeight)
                            break;
                        Height = pVerticalChange;
                        break;
                }
            };

            border.MouseLeftButtonUp += (sender, e) => border.ReleaseMouseCapture();
        }
        /// <summary>
        /// Registers all the borders (calls <see cref="RegisterBorderEvents(WindowBorderEdge, Border)"/> for all borders)
        /// </summary>
        void RegisterBorders() {
            RegisterBorderEvents(WindowBorderEdge.Left, (Border)GetTemplateChild("WindowBorderLeft"));
            RegisterBorderEvents(WindowBorderEdge.TopLeft, (Border)GetTemplateChild("WindowBorderTopLeft"));
            RegisterBorderEvents(WindowBorderEdge.Top, (Border)GetTemplateChild("WindowBorderTop"));
            RegisterBorderEvents(WindowBorderEdge.TopRight, (Border)GetTemplateChild("WindowBorderTopRight"));
            RegisterBorderEvents(WindowBorderEdge.Right, (Border)GetTemplateChild("WindowBorderRight"));
            RegisterBorderEvents(WindowBorderEdge.BottomRight, (Border)GetTemplateChild("WindowBorderBottomRight"));
            RegisterBorderEvents(WindowBorderEdge.Bottom, (Border)GetTemplateChild("WindowBorderBottom"));
            RegisterBorderEvents(WindowBorderEdge.BottomLeft, (Border)GetTemplateChild("WindowBorderBottomLeft"));
        }
        void RegisterInnerBorder() {
            var innerBorder = (Border)GetTemplateChild("WindowInnerBorder");
            innerBorder.MouseLeftButtonDown += (sender, e) => {
                _restoreTop = e.GetPosition(this).Y;

                if(e.ClickCount == 2 && e.ChangedButton == MouseButton.Left && (ResizeMode != ResizeMode.CanMinimize && ResizeMode != ResizeMode.NoResize))
                    WindowState = WindowState != WindowState.Maximized ? WindowState.Maximized : WindowState.Normal;
                else
                    DragMove();
            };
            innerBorder.MouseMove += (sender, e) => {
                if(e.LeftButton != MouseButtonState.Pressed || !innerBorder.IsMouseOver || WindowState != WindowState.Maximized)
                    return;

                WindowState = WindowState.Normal;
                Top = _restoreTop - 10;

                // Center the window on the mouse
                var mousePos = PointToScreen(e.GetPosition(this));
                var newLeft = mousePos.X - (ActualWidth / 2);
                System.Diagnostics.Debug.WriteLine("MouseLeft: " + mousePos.X + "; WindowLeft: " + newLeft + "; Width(A/N): " + ActualWidth + "/" + Width);
                Left = newLeft > 0 ? 0 : newLeft;
                DragMove();
            };
        }
        #endregion

        enum WindowBorderEdge {
            Left,
            TopLeft,
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft
        }

        #region NativeMethods
        static class NativeMethods {
            [DllImport("user32")]
            internal static extern bool GetMonitorInfo(IntPtr hMonitor, Monitorinfo lpmi);

            [DllImport("user32")]
            internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flags);
        }
        [StructLayout(LayoutKind.Sequential)]
        struct POINT {
            public int x;
            public int y;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct Minmaxinfo {
            readonly POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            readonly POINT ptMinTrackSize;
            readonly POINT ptMaxTrackSize;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        class Monitorinfo {
            public int cbSize = Marshal.SizeOf(typeof(Minmaxinfo));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        struct RECT {
            public readonly int left;
            public readonly int top;
            public readonly int right;
            public readonly int bottom;
        }
        #endregion
    }
}