using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public AppConfig draftConfig { get; }
        public ObservableCollection<ParameterRow> CallParameterRows { get; } = new();
        public ObservableCollection<ParameterRow> PutParameterRows { get; } = new();

        public SettingsWindow(AppConfig draft)
        {
            InitializeComponent();
            draftConfig = draft;
            DataContext = draft;

            Logger.Log("SettingsWindow initialized with draft config.");
            // dictionary -> editable rows (this is the draft copy, so edits are temporary)
            foreach (var kv in draft.CallParametersToMonitor)
                CallParameterRows.Add(new ParameterRow { Parameter = kv.Key, Value = kv.Value });
            foreach (var kv in draft.PutParametersToMonitor)
                PutParameterRows.Add(new ParameterRow { Parameter = kv.Key, Value = kv.Value });

            // hand the rows to the grids (simplest way — no RelativeSource needed)
            CallParametersGrid.ItemsSource = CallParameterRows;
            PutParametersGrid.ItemsSource = PutParameterRows;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Log("Saving settigns config...");
                draftConfig.CallParametersToMonitor = CallParameterRows
                .Where(r => !string.IsNullOrWhiteSpace(r.Parameter))
                .ToDictionary(r => r.Parameter, r => r.Value);

                draftConfig.PutParametersToMonitor = PutParameterRows
                    .Where(r => !string.IsNullOrWhiteSpace(r.Parameter))
                    .ToDictionary(r => r.Parameter, r => r.Value);

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

    public class ParameterRow
    {
        public string Parameter { get; set; } = "";  // the dictionary Key
        public string Value { get; set; } = "";       // the dictionary Value (RTD code)
    }
}
