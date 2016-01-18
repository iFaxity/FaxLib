using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FaxLib.WPF.Controls {
    /// <summary>
    /// Interaction logic for NumericBox.xaml
    /// </summary>
    public partial class NumericBox : UserControl {
        #region Properties
        public double Value {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); OnValueChanged(); }
        }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(NumericBox), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(ValueProperty_Changed)));
        static void ValueProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var sender = obj as NumericBox;

            if(obj != null) {
                sender.Value = (double)e.NewValue;
                sender.Update();
            }
        }

        public double Steps {
            get { return (double)GetValue(StepsProperty); }
            set { SetValue(StepsProperty, value); OnValueChanged(); }
        }
        public static readonly DependencyProperty StepsProperty = DependencyProperty.Register("Steps", typeof(double), typeof(NumericBox), new FrameworkPropertyMetadata(1d, new PropertyChangedCallback(StepsProperty_Changed)));
        static void StepsProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var sender = obj as NumericBox;

            if(obj != null) {
                sender.Steps = (double)e.NewValue;
                sender.Update();
            }
        }

        public double MinValue {
            get { return (double)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); OnValueChanged(); }
        }
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(double), typeof(NumericBox), new FrameworkPropertyMetadata(1d, new PropertyChangedCallback(MinValueProperty_Changed)));
        static void MinValueProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var sender = obj as NumericBox;

            if(obj != null) {
                sender.MinValue = (double)e.NewValue;
                sender.Update();
            }
        }

        public double MaxValue {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); OnValueChanged(); }
        }
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(NumericBox), new FrameworkPropertyMetadata(1d, new PropertyChangedCallback(MaxValueProperty_Changed)));
        static void MaxValueProperty_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var sender = obj as NumericBox;

            if(obj != null) {
                sender.MaxValue = (double)e.NewValue;
                sender.Update();
            }
        }

        #endregion

        public event EventHandler ValueChanged;
        protected virtual void OnValueChanged() {
            if(ValueChanged != null) ValueChanged(this, new EventArgs());
        }

        public NumericBox() {
            InitializeComponent();

            bool loop = true;
            int count = 1;
            while(loop) {
                string s = "";
                for(int i = 0; i < count; i++) {
                    s += "0";
                }
                var size = new FormattedText(s, System.Globalization.CultureInfo.CurrentCulture, textBox.FlowDirection, new Typeface(textBox.FontFamily, textBox.FontStyle, textBox.FontWeight, textBox.FontStretch), textBox.FontSize, textBox.Foreground);

                if(size.Width >= textBox.Width) { textBox.MaxLength = count - 1; loop = false; }
                else count++;
            }
        }

        void _TextInput(object sender, TextCompositionEventArgs e) {
            double d;
            if(double.TryParse(textBox.Text, out d)) if(d == 0) textBox.Clear();
            if(double.TryParse(e.Text, out d)) { e.Handled = false; OnValueChanged(); }
            else e.Handled = true;
        }
        void _KeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Up) Up(this, new RoutedEventArgs());
            else if(e.Key == Key.Down) Down(this, new RoutedEventArgs());
        }
        void Up(object sender, RoutedEventArgs e) {
            Value = double.Parse(textBox.Text) + Steps;
            textBox.Text = "" + Value;
            OnValueChanged();
        }
        void Down(object sender, RoutedEventArgs e) {
            Value = double.Parse(textBox.Text) - Steps;
            textBox.Text = "" + Value;
            OnValueChanged();
        }

        internal void Update() {

        }
    }
}
