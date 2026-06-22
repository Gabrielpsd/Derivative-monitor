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
using System.Windows.Shapes;

namespace DerivativeMonitor
{
    /// <summary>
    /// Interaction logic for ColourSettingsWindow.xaml
    /// </summary>
    public partial class ColourSettingsWindow : Window
    {
        public AppConfig draftConfig { get; }

        public ColourSettingsWindow(AppConfig draft)
        {
            InitializeComponent();
            draftConfig = draft;
            DataContext = draft;

            // dictionary -> editable rows (this is the draft copy, so edits are temporary)

        }

        private void PickColor_Click(object sender, RoutedEventArgs e)
        {

            Logger.Log("PickColor_Click called");
            var target = (System.Windows.Controls.TextBox)((System.Windows.Controls.Button)sender).Tag;   // the row's hex TextBox

            using var dlg = new System.Windows.Forms.ColorDialog { FullOpen = true };

            // seed the dialog with the current color if it's valid
            try
            {
                var c = (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                            .ConvertFromString(target.Text);
                dlg.Color = System.Drawing.Color.FromArgb(c.R, c.G, c.B);
            }
            catch { /* empty/invalid hex — just open with default */ }

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var p = dlg.Color;
                target.Text = $"#{p.R:X2}{p.G:X2}{p.B:X2}";  // writing the text updates swatch + preview live
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("Saving colour settings config...");
            try
            {

                ConfigManager.Validate(draftConfig);

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
        }

    }

}
