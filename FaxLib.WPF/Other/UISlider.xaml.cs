using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FaxLib.WPF.Controls {
    /// <summary>
    /// Interaction logic for UISlider.xaml
    /// </summary>
    public partial class UISlider : UserControl {
        #region Dependency Property
        //Orientation locked due to no support in Vertical slider yet
        public Orientation Orientation {
            get { return (Orientation)GetValue(OrientationProperty); }
            set {
                SetValue(OrientationProperty, value);
                //offset = value == Orientation.Horizontal ? thumb.Width : thumb.Height;
                Update();
            }
        }
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(UISlider), new PropertyMetadata(Orientation.Horizontal, new PropertyChangedCallback(OrientationProperty_Changed)));
        static void OrientationProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var sender = (UISlider)obj;
            if(sender == null) return;

            sender.Orientation = (Orientation)e.NewValue;

            if(sender.Orientation == Orientation.Horizontal) {
                sender.fill.VerticalAlignment = VerticalAlignment.Stretch;
                sender.fill.HorizontalAlignment = HorizontalAlignment.Right;
                sender.fill.SetBinding(WidthProperty, new Binding("ActualWidth") { ElementName = "userControl" });
            }
            else {
                sender.fill.VerticalAlignment = VerticalAlignment.Top;
                sender.fill.HorizontalAlignment = HorizontalAlignment.Stretch;
                sender.fill.SetBinding(HeightProperty, new Binding("ActualHeight") { ElementName = "userControl" });
            }

            //sender.offset = (Orientation)e.NewValue == Orientation.Horizontal ? sender.thumb.Width : sender.thumb.Height;
            sender.Update();
        }

        public double Value {
            get { return (double)GetValue(ValueProperty); }
            set {
                if(value < MinValue) value = MinValue;
                else if(value > MaxValue) value = MaxValue;

                OnValueChanged(value);
                SetValue(ValueProperty, value);
                Update();
            }
        }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(UISlider), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(ValueProperty_Changed)));
        static void ValueProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var sender = (UISlider)obj;
            sender.Value = (double)e.NewValue;
            if(sender != null) sender.Update();
        }

        public double MaxValue {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); Update(); }
        }
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(UISlider), new FrameworkPropertyMetadata(100d, new PropertyChangedCallback(MaxValueProperty_Changed)));
        static void MaxValueProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var sender = (UISlider)obj;
            sender.MaxValue = (double)e.NewValue;
            if(sender != null) sender.Update();
        }

        public double MinValue {
            get { return (double)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); Update(); }
        }
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(double), typeof(UISlider), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(MinValueProperty_Changed)));
        static void MinValueProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var sender = (UISlider)obj;
            sender.MinValue = (double)e.NewValue;
            if(sender != null) sender.Update();
        }

        public bool UpdateOnRelease {
            get { return (bool)GetValue(UpdateOnReleaseProperty); }
            set { SetValue(UpdateOnReleaseProperty, value); Update(); }
        }
        public static readonly DependencyProperty UpdateOnReleaseProperty = DependencyProperty.Register("UpdateOnRelease", typeof(bool), typeof(UISlider), new FrameworkPropertyMetadata(true));

        #endregion

        public event EventHandler<RoutedPropertyChangedEventArgs<double>> ValueChanged;
        protected virtual void OnValueChanged(double newValue) {
            if(ValueChanged != null) ValueChanged(this, new RoutedPropertyChangedEventArgs<double>(Value, newValue));
        }

        public UISlider() {
            InitializeComponent();

            Loaded += (sender, e) => {
                Window wind = Window.GetWindow(this);
                if(wind == null) return;

                wind.PreviewMouseMove += _MouseMove;
                wind.PreviewMouseUp += (a, b) => active = false;
                Update();
            };
        }
        bool active = false; //Fixes the sliding bug with outside control

        //Bar Update Function
        public void Update() {
            if(Orientation == Orientation.Horizontal) fill.Width = ActualWidth - (ActualWidth / MaxValue) * Value;
            else fill.Height = ActualHeight - (ActualHeight / MaxValue) * Value;
        }
        //Moves the slider then updates it
        void MoveSlider(Point pt) {
            if(Orientation == Orientation.Horizontal) Value = (pt.X / ActualWidth) * MaxValue;
            else Value = ((ActualHeight - pt.Y) / ActualHeight) * MaxValue;

            Update();
        }
        void _MouseMove(object sender, MouseEventArgs e) {
            if(e.MouseDevice.LeftButton == MouseButtonState.Pressed && active) MoveSlider(e.GetPosition(this));
        }
        void _MouseDown(object sender, MouseButtonEventArgs e) {
            active = true;
            MoveSlider(e.GetPosition(this));
        }

    }
}
