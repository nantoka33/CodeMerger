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
using System.Windows.Shapes;

namespace CodeMerger
{
    /// <summary>
    /// SettingsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private Settings settings;

        public SettingsWindow(Settings currentSettings)
        {
            InitializeComponent();
            settings = currentSettings;
            ExcludeTextBox.Text = string.Join(Environment.NewLine, settings.ExcludeFiles);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            settings.ExcludeFiles = ExcludeTextBox.Text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            SettingsService.Save(settings);
            this.Close();
        }
    }
}
