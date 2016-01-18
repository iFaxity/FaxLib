using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FaxLib.WPF.Controls {
    /// <summary>
    /// Interaction logic for ProgressBar.xaml
    /// </summary>
    public partial class Pbar : UserControl {
        #region Dependency Properties
        public double Value {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); Update(); }
        }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(Pbar), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(ValueProperty_Changed)));
        static void ValueProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            Pbar bar = obj as Pbar;
            bar.Value = (double)e.NewValue;
            if(bar != null)
                bar.Update();
        }

        public bool ShowDecimals {
            get { return (bool)GetValue(ShowDecimalsProperty); }
            set { SetValue(ShowDecimalsProperty, value); }
        }
        public static readonly DependencyProperty ShowDecimalsProperty = DependencyProperty.Register("ShowDecimals", typeof(bool), typeof(Pbar), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(ShowDecimalsProperty_Changed)));
        static void ShowDecimalsProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            Pbar bar = obj as Pbar;
            bar.ShowDecimals = (bool)e.NewValue;
            if(bar != null)
                bar.Update();
        }

        public Visibility TextVisibility {
            get { return (Visibility)GetValue(TextVisibilityProperty); }
            set { SetValue(TextVisibilityProperty, value); }
        }
        public static readonly DependencyProperty TextVisibilityProperty = DependencyProperty.Register("TextVisibility", typeof(Visibility), typeof(Pbar), new FrameworkPropertyMetadata(Visibility.Visible, new PropertyChangedCallback(TextVisibilityProperty_Changed)));
        static void TextVisibilityProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            Pbar bar = obj as Pbar;
            bar.TextVisibility = (Visibility)e.NewValue;
            if(bar != null)
                bar.Update();
        }

        public Brush Fill {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(Pbar), new FrameworkPropertyMetadata(new BrushConverter().ConvertFromString("#FF0097FB"), new PropertyChangedCallback(FillProperty_Changed)));
        static void FillProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            Pbar bar = obj as Pbar;
            bar.Fill = (Brush)e.NewValue;
            if(bar != null)
                bar.Update();
        }

        public double MaxValue {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(Pbar), new FrameworkPropertyMetadata(100d, new PropertyChangedCallback(MaxValueProperty_Changed)));
        static void MaxValueProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            Pbar bar = obj as Pbar;
            bar.MaxValue = (double)e.NewValue;
            if(bar != null)
                bar.Update();
        }

        public double MinValue {
            get { return (double)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(double), typeof(Pbar), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(MinValueProperty_Changed)));
        static void MinValueProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            Pbar bar = obj as Pbar;
            bar.MinValue = (double)e.NewValue;
            if(bar != null)
                bar.Update();
        }
        #endregion

        public Pbar() {
            InitializeComponent();
            SizeChanged += (sender, e) => Update();
        }

        void Update() {
            fill.Fill = Fill;

            if(Value < MinValue)
                Value = MinValue;
            else if(Value > MaxValue)
                Value = MaxValue;
            fill.Width = (ActualWidth / MaxValue) * Value;

            if(TextVisibility == Visibility.Visible) {
                double perc = (100 / MaxValue) * Value;

                if(ShowDecimals) txb.Text = perc + " %";
                else txb.Text = (int)perc + " %";
            }
            else if(!string.IsNullOrWhiteSpace(txb.Text)) txb.Text = "";
        }
    }
}
