using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace FaxLib.WPF.Controls {
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : UserControl {
        #region Properties

        #region Brushes
        [Category("TextBox")]
        public Brush PlaceHolderBrush {
            get { return (Brush)GetValue(PlaceholderBrushProperty); }
            set { SetValue(PlaceholderBrushProperty, value); }
        }
        public static readonly DependencyProperty PlaceholderBrushProperty = DependencyProperty.Register("PlaceholderBrush", typeof(Brush), typeof(InputBox), new PropertyMetadata(new BrushConverter().ConvertFromString("#FF3F3F46")));
        [Category("TextBox")]
        public string PlaceHolder {
            get { return (string)GetValue(PlaceHolderProperty); }
            set { SetValue(PlaceHolderProperty, value); }
        }
        public static readonly DependencyProperty PlaceHolderProperty = DependencyProperty.Register("PlaceHolder", typeof(string), typeof(InputBox), new PropertyMetadata(null, new PropertyChangedCallback(PlaceHolderProperty_Changed)));
        static void PlaceHolderProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var sender = (InputBox)obj;
            if(sender != null) {
                sender.PlaceHolder = (string)e.NewValue;
                sender.Update(sender);
            }
        }

        [Category("TextBox")]
        public Brush HoverBorderBrush {
            get { return (Brush)GetValue(HoverBorderBrushProperty); }
            set { SetValue(HoverBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty HoverBorderBrushProperty = DependencyProperty.Register("HoverBorderBrush", typeof(Brush), typeof(InputBox), new PropertyMetadata(new BrushConverter().ConvertFromString("#FF007ACC")));
        [Category("TextBox")]
        public Brush HoverBackground {
            get { return (Brush)GetValue(HoverBackgroundProperty); }
            set { SetValue(HoverBackgroundProperty, value); }
        }
        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register("HoverBackground", typeof(Brush), typeof(InputBox), new PropertyMetadata(new BrushConverter().ConvertFromString("#FF3F3F46")));
        [Category("TextBox")]
        public Brush HoverForeground {
            get { return (Brush)GetValue(HoverForegroundProperty); }
            set { SetValue(HoverForegroundProperty, value); }
        }
        public static readonly DependencyProperty HoverForegroundProperty = DependencyProperty.Register("HoverForeground", typeof(Brush), typeof(InputBox), new PropertyMetadata(new BrushConverter().ConvertFromString("#FFF1F1F1")));

        [Category("TextBox")]
        public Brush FocusBorderBrush {
            get { return (Brush)GetValue(FocusBorderBrushProperty); }
            set { SetValue(FocusBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty FocusBorderBrushProperty = DependencyProperty.Register("FocusBorderBrush", typeof(Brush), typeof(InputBox), new PropertyMetadata(new BrushConverter().ConvertFromString("#FF007ACC")));
        [Category("TextBox")]
        public Brush FocusBackground {
            get { return (Brush)GetValue(FocusBackgroundProperty); }
            set { SetValue(FocusBackgroundProperty, value); }
        }
        public static readonly DependencyProperty FocusBackgroundProperty = DependencyProperty.Register("FocusBackground", typeof(Brush), typeof(InputBox), new PropertyMetadata(new BrushConverter().ConvertFromString("#FF3F3F46")));
        [Category("TextBox")]
        public Brush FocusForeground {
            get { return (Brush)GetValue(FocusForegroundProperty); }
            set { SetValue(FocusForegroundProperty, value); }
        }
        public static readonly DependencyProperty FocusForegroundProperty = DependencyProperty.Register("FocusForeground", typeof(Brush), typeof(InputBox), new PropertyMetadata(new BrushConverter().ConvertFromString("#FFF1F1F1")));

        [Category("TextBox")]
        public Brush DisabledBorderBrush {
            get { return (Brush)GetValue(DisabledBorderBrushProperty); }
            set { SetValue(DisabledBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty DisabledBorderBrushProperty = DependencyProperty.Register("DisabledBorderBrush", typeof(Brush), typeof(InputBox), new PropertyMetadata(new BrushConverter().ConvertFromString("#FF3F3F46")));
        [Category("TextBox")]
        public Brush DisabledBackground {
            get { return (Brush)GetValue(DisabledBackgroundProperty); }
            set { SetValue(DisabledBackgroundProperty, value); }
        }
        public static readonly DependencyProperty DisabledBackgroundProperty = DependencyProperty.Register("DisabledBackground", typeof(Brush), typeof(InputBox), new PropertyMetadata(new BrushConverter().ConvertFromString("#FF999999")));
        [Category("TextBox")]
        public Brush DisabledForeground {
            get { return (Brush)GetValue(DisabledForegroundProperty); }
            set { SetValue(DisabledForegroundProperty, value); }
        }
        public static readonly DependencyProperty DisabledForegroundProperty = DependencyProperty.Register("DisabledForeground", typeof(Brush), typeof(InputBox), new PropertyMetadata(new BrushConverter().ConvertFromString("#FF999999")));
        #endregion

        [Category("TextBox")]
        public string Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(InputBox), new PropertyMetadata(null, new PropertyChangedCallback(TextProperty_Changed)));
        static void TextProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var sender = (InputBox)obj;
            if(sender != null) {
                sender.Text = (string)e.NewValue;
                sender.Update(sender);
            }
        }
        [Category("TextBox")]
        public bool IsPassword {
            get { return (bool)GetValue(IsPasswordProperty); }
            set { SetValue(IsPasswordProperty, value); }
        }
        public static readonly DependencyProperty IsPasswordProperty = DependencyProperty.Register("IsPassword", typeof(bool), typeof(InputBox), new PropertyMetadata(false, new PropertyChangedCallback(IsPasswordProperty_Changed)));
        static void IsPasswordProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var sender = (InputBox)obj;
            if(sender != null) {
                sender.IsPassword = (bool)e.NewValue;
                sender.Update(sender);
            }
        }
        [Category("TextBox")]
        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(InputBox), new PropertyMetadata(false, new PropertyChangedCallback(IsReadOnlyProperty_Changed)));
        static void IsReadOnlyProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var sender = (InputBox)obj;
            if(sender != null) {
                sender.IsReadOnly = (bool)e.NewValue;
                sender.Update(sender);
            }
        }
        [Category("TextBox")]
        public char PassChar {
            get { return (char)GetValue(PassCharProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
        public static readonly DependencyProperty PassCharProperty = DependencyProperty.Register("PassChar", typeof(char), typeof(InputBox), new PropertyMetadata('●', new PropertyChangedCallback(PassCharProperty_Changed)));
        static void PassCharProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var sender = (InputBox)obj;
            if(sender != null) {
                sender.PassChar = (char)e.NewValue;
                sender.Update(sender);
            }
        }
        #endregion

        public InputBox() {
            InitializeComponent();

            tbx.TextInput += (sender, e) => {
                Text.Insert(tbx.CaretIndex, e.Text);
                tbx.Text.Insert(tbx.CaretIndex, PassChar.ToString());
            };

            tbx.PreviewKeyDown += (sender, e) => {
                switch(e.Key) {
                    case Key.Enter:
                        tbx.Text.Insert(tbx.CaretIndex, "\n");
                        break;
                    case Key.Delete:
                        tbx.Text.Remove(tbx.CaretIndex);
                        break;
                    case Key.Back:
                        tbx.Text.Remove(tbx.CaretIndex - 1);
                        break;
                }
                Text = tbx.Text;
            };
            // Hover Style
            tbx.MouseEnter += (sender, e) => {
                if(IsEnabled) {
                    tbx.Background = HoverBackground;
                    tbx.BorderBrush = HoverBorderBrush;
                    tbx.Foreground = HoverForeground;
                }
            };
            // Default Style if not focused
            tbx.MouseLeave += (sender, e) => {
                if(IsEnabled || IsFocused) {
                    tbx.Background = Background;
                    tbx.BorderBrush = BorderBrush;
                    tbx.Foreground = Foreground;
                }
            };
            // Focus Style
            tbx.GotFocus += (sender, e) => {
                if(IsEnabled) {
                    tbx.Background = FocusBackground;
                    tbx.BorderBrush = FocusBorderBrush;
                    tbx.Foreground = FocusForeground;
                    tbx.Text = "";
                }
            };
            // Default Style
            tbx.LostFocus += (sender, e) => {
                if(IsEnabled) {
                    tbx.Background = Background;
                    tbx.BorderBrush = BorderBrush;
                    tbx.Foreground = Foreground;
                    if(tbx.Text.Length < 1)
                        tbx.Text = PlaceHolder;
                }
            };

            Update(this);
        }

        public void Update(InputBox input) {
            tbx.Text = Text != null && Text.Length > 0 ? Text : PlaceHolder;
        }
    }
}
