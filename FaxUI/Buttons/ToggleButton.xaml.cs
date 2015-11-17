using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace FaxUI {
    /// <summary>
    /// Interaction logic for UIToggleButton.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class ToggleButton {
        public static readonly DependencyProperty IsToggledProperty = DependencyProperty.Register("IsToggled", typeof(bool), typeof(ToggleButton), new PropertyMetadata(true));
        bool IsToggled {
            get { return (bool)GetValue(IsToggledProperty); }
            set { SetValue(IsToggledProperty, value); OnToggleChanged(); }
        }

        static readonly BrushConverter BrushConverter = new BrushConverter();

        public static readonly DependencyProperty ToggledBackgroundProperty = DependencyProperty.Register("ToggledBackground", typeof(Brush), typeof(ToggleButton), new PropertyMetadata(BrushConverter.ConvertFrom("#00000000")));
        Brush ToggledBackground {
            get { return GetValue(ToggledBackgroundProperty) as Brush; }
            set { SetValue(ToggledBackgroundProperty, value); }
        }

        public static readonly DependencyProperty ToggledForegroundProperty = DependencyProperty.Register("ToggledForeground", typeof(Brush), typeof(ToggleButton), new PropertyMetadata(BrushConverter.ConvertFrom("#00000000")));
        Brush ToggledForeground {
            get { return GetValue(ToggledForegroundProperty) as Brush; }
            set { SetValue(ToggledForegroundProperty, value); }
        }

        public static readonly DependencyProperty ToggledBorderBrushProperty = DependencyProperty.Register("ToggledBorderBrush", typeof(Brush), typeof(ToggleButton), new PropertyMetadata(BrushConverter.ConvertFrom("#00000000")));
        Brush ToggledBorderBrush {
            get { return GetValue(ToggledBorderBrushProperty) as Brush; }
            set { SetValue(ToggledBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register("HoverBackground", typeof(Brush), typeof(ToggleButton), new PropertyMetadata(BrushConverter.ConvertFrom("#00000000")));
        Brush HoverBackground {
            get { return GetValue(HoverBackgroundProperty) as Brush; }
            set { SetValue(HoverBackgroundProperty, value); }
        }
        public static readonly DependencyProperty HoverForegroundProperty = DependencyProperty.Register("HoverForeground", typeof(Brush), typeof(ToggleButton), new PropertyMetadata(BrushConverter.ConvertFrom("#00000000")));
        Brush HoverForeground {
            get { return GetValue(HoverForegroundProperty) as Brush; }
            set { SetValue(HoverForegroundProperty, value); }
        }
        public static readonly DependencyProperty HoverBorderBrushProperty = DependencyProperty.Register("HoverBorderBrush", typeof(Brush), typeof(ToggleButton), new PropertyMetadata(BrushConverter.ConvertFrom("#00000000")));
        Brush HoverBorderBrush {
            get { return GetValue(HoverBorderBrushProperty) as Brush; }
            set { SetValue(HoverBorderBrushProperty, value); }
        }

        public event RoutedEventHandler ToggleChanged;
        readonly RoutedEvent _toggledChangedEvent = EventManager.RegisterRoutedEvent("ToggledChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToggleButton));
        protected virtual void OnToggleChanged() {
            IsToggled = !IsToggled;

            if(ToggleChanged != null) ToggleChanged(this, new RoutedEventArgs(_toggledChangedEvent));
        }

        public ToggleButton() {
            InitializeComponent();
        }
    }
}
