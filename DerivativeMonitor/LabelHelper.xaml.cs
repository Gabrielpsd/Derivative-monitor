using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DerivativeMonitor
{
    /// <summary>
    /// Interaction logic for LabelHelper.xaml
    /// </summary>
   
    public partial class LabelHelper : System.Windows.Controls.UserControl
    {
        public LabelHelper()
        {
            InitializeComponent();
        }

        // The field label, e.g. "Ticker"
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register(nameof(LabelText), typeof(string),
                typeof(LabelHelper), new PropertyMetadata(""));

        public string LabelText
        {
            get => (string)GetValue(LabelTextProperty);
            set => SetValue(LabelTextProperty, value);
        }

        // The tooltip text shown when hovering the "?"
        public static readonly DependencyProperty HelpTextProperty =
            DependencyProperty.Register(nameof(HelpText), typeof(string),
                typeof(LabelHelper), new PropertyMetadata(""));

        public string HelpText
        {
            get => (string)GetValue(HelpTextProperty);
            set => SetValue(HelpTextProperty, value);
        }
    }
    
}
