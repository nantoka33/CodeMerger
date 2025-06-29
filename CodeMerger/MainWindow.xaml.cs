using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace CodeMerger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// property
        /// </summary>
        private List<string> ExcludeFileNm = new();
        private Settings AppSettings;

        /// <summary>
        /// 初期化
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            AppSettings = SettingsService.Load();
            ExcludeFileNm = AppSettings.ExcludeFiles;
        }

        /// <summary>
        /// 実行ボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Execute_Click(object sender, RoutedEventArgs e)
        {
            var inputDir = InputPathTextBox.Text;
            var outputDir = OutputPathTextBox.Text;

            if(string.IsNullOrWhiteSpace(inputDir))
            {
                LogTextBox.Text = "取込元のフォルダを指定してください。";
                return;
            }

            if(string.IsNullOrWhiteSpace(outputDir))
            {
                LogTextBox.Text = "出力先のフォルダが空白のため、「C:\\output_」に出力します。";
                outputDir = @"C:\output_";
            }
            LogTextBox.Clear();

            if (!Directory.Exists(inputDir))
            {
                LogTextBox.Text = "取込元フォルダが存在しません。";
                return;
            }

            Directory.CreateDirectory(outputDir);

            var excludeFiles = new HashSet<string>(ExcludeFileNm, StringComparer.OrdinalIgnoreCase);

            // 全サブフォルダ含めて取得（再帰的）
            var allDirs = Directory.GetDirectories(inputDir, "*", SearchOption.AllDirectories)
                                                    .Where(path =>
                                                    {
                                                        var dirName = Path.GetFileName(path);
                                                        return !string.Equals(dirName, "bin", StringComparison.OrdinalIgnoreCase) &&
                                                                !string.Equals(dirName, "obj", StringComparison.OrdinalIgnoreCase);
                                                    })
                                                    .ToList();


            // 入力ディレクトリ直下も対象に含める
            allDirs.Insert(0, inputDir);

            foreach (var dir in allDirs)
            {
                var folderName = Path.GetFileName(dir);
                if (string.IsNullOrEmpty(folderName)) continue;

                var csBuilder = new StringBuilder();
                var xamlBuilder = new StringBuilder();
                var vbBuilder = new StringBuilder();

                var files = Directory.GetFiles(dir, "*.*")
                    .Where(f => (f.EndsWith(".cs") || f.EndsWith(".xaml") || f.EndsWith(".xaml.cs") || f.EndsWith(".vb")) &&
                                !excludeFiles.Any(x => Path.GetFileName(f).Contains(x)))
                    .ToList();

                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string content = File.ReadAllText(file);

                    if (fileName.EndsWith(".cs") && !fileName.EndsWith(".xaml.cs"))
                    {
                        csBuilder.AppendLine($"// --- {fileName} ---");
                        csBuilder.AppendLine(content);
                        csBuilder.AppendLine();
                    }
                    else if (fileName.EndsWith(".xaml"))
                    {
                        xamlBuilder.AppendLine($"// --- {fileName} ---");
                        xamlBuilder.AppendLine(content);
                        xamlBuilder.AppendLine();
                    }
                    else if (fileName.EndsWith(".xaml.cs"))
                    {
                        csBuilder.AppendLine($"// --- {fileName} ---");
                        csBuilder.AppendLine(content);
                        csBuilder.AppendLine();
                    }
                    else if (fileName.EndsWith(".vb"))
                    {
                        vbBuilder.AppendLine($"// --- {fileName} ---");
                        vbBuilder.AppendLine(content);
                        vbBuilder.AppendLine();
                    }
                }

                if (csBuilder.Length > 0)
                {
                    var outPath = Path.Combine(outputDir, $"{folderName}_cs.txt");
                    File.WriteAllText(outPath, csBuilder.ToString());
                    LogTextBox.AppendText($"出力: {outPath}\n");
                }

                if (xamlBuilder.Length > 0)
                {
                    var outPath = Path.Combine(outputDir, $"{folderName}_xaml.txt");
                    File.WriteAllText(outPath, xamlBuilder.ToString());
                    LogTextBox.AppendText($"出力: {outPath}\n");
                }

                if (vbBuilder.Length > 0)
                {
                    var outPath = Path.Combine(outputDir, $"{folderName}_vb.txt");
                    File.WriteAllText(outPath, vbBuilder.ToString());
                    LogTextBox.AppendText($"出力: {outPath}\n");
                }
            }

            LogTextBox.AppendText("出力が完了しました。\n");
        }

        /// <summary>
        /// 取込元フォルダの参照ボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseInput_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = "取込元のフォルダを選択してください";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InputPathTextBox.Text = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// 出力先フォルダの参照ボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = "出力先のフォルダを選択してください";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OutputPathTextBox.Text = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// 出力先フォルダを開くボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutputFileOpen_Click(object sender, RoutedEventArgs e)
        {
            var outputPath = OutputPathTextBox.Text;
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                //LogTextBox.Text = "出力先のフォルダが存在しません。";
                outputPath = @"C:\output_";
            }

            if (Directory.Exists(outputPath))
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = outputPath,
                    UseShellExecute = true
                });
            }
            else
            {
                LogTextBox.Text = "出力先のフォルダが存在しません。";
            }
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(AppSettings);
            settingsWindow.ShowDialog();

            // 再読み込み
            AppSettings = SettingsService.Load();
            ExcludeFileNm = AppSettings.ExcludeFiles;
        }

    }
}