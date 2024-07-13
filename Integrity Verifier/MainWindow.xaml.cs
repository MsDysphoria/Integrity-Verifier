using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using System.Threading;

namespace Integrity_Verifier
{
    public partial class MainWindow : Window
    {
        private BitmapImage discordD;
        private BitmapImage discordH;
        private BitmapImage patreonD;
        private BitmapImage patreonH;
        private BitmapImage githubD;
        private BitmapImage githubH;
        private BitmapImage webD;
        private BitmapImage webH;
        private BitmapImage deviantArtD;
        private BitmapImage deviantArtH;
        private BitmapImage artstationD;
        private BitmapImage artstationH;
        private BitmapImage buttonD;
        private BitmapImage buttonH;
        private BitmapImage enabledButtonImage;

        private string savedLogFilePath;

        private string[] mismatchedFiles;
        private string[] excessFiles;
        private string[] missingFiles;

        private bool directorySelected = false;
        private bool fileSelected = false;

        private CancellationTokenSource cancellationTokenSource;
        public MainWindow()
        {
            InitializeComponent();

            enabledButtonImage = new BitmapImage(new Uri("/Images/ButtonEnabled.png", UriKind.Relative));
            discordD = new BitmapImage(new Uri("/Images/Discord.png", UriKind.Relative));
            discordH = new BitmapImage(new Uri("/Images/Discord_H.png", UriKind.Relative));
            patreonD = new BitmapImage(new Uri("/Images/Patreon.png", UriKind.Relative));
            patreonH = new BitmapImage(new Uri("/Images/Patreon_H.png", UriKind.Relative));
            githubD = new BitmapImage(new Uri("/Images/Github.png", UriKind.Relative));
            githubH = new BitmapImage(new Uri("/Images/Github_H.png", UriKind.Relative));
            webD = new BitmapImage(new Uri("/Images/Website.png", UriKind.Relative));
            webH = new BitmapImage(new Uri("/Images/Website_H.png", UriKind.Relative));
            deviantArtD = new BitmapImage(new Uri("/Images/DeviantArt.png", UriKind.Relative));
            deviantArtH = new BitmapImage(new Uri("/Images/DeviantArt_H.png", UriKind.Relative));
            artstationD = new BitmapImage(new Uri("/Images/Artstation.png", UriKind.Relative));
            artstationH = new BitmapImage(new Uri("/Images/Artstation_H.png", UriKind.Relative));
            buttonD = new BitmapImage(new Uri("/Images/GeneralButton.png", UriKind.Relative));
            buttonH = new BitmapImage(new Uri("/Images/GeneralButton_H.png", UriKind.Relative));

            mismatchedFiles = new string[0];
            excessFiles = new string[0];
            missingFiles = new string[0];
        }

        // Basic Buttons
        private void btnInfo_Click(object sender, RoutedEventArgs e)
        { ShowInformation(); }
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        { System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized; }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        { System.Windows.Application.Current.Shutdown(); }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        { if (e.LeftButton == MouseButtonState.Pressed) { DragMove(); } }

        private void EnableCheckIntegrity()
        {
            if (fileSelected && directorySelected)
            {
                CheckIntegrityBtn.Source = enabledButtonImage;
                CheckIntegrityBtn.IsEnabled = true;
            }
        }
        private void TargetFolderForExport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog();
            folderDialog.IsFolderPicker = true;
            folderDialog.Title = "Select the target folder for export";

            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ExportConsole.Text = folderDialog.FileName;
                ExportBtn.Source = enabledButtonImage;
                ExportBtn.IsEnabled = true;
                BG1.Opacity = 0.15;
            }
        }


        private void ExportTargetFolder(object sender, MouseButtonEventArgs e)
        {
            try
            {
                CommonSaveFileDialog saveDialog = new CommonSaveFileDialog();
                saveDialog.Title = "Save .intgf file";
                saveDialog.DefaultExtension = ".intgf";
                saveDialog.Filters.Add(new CommonFileDialogFilter("INTGF Files", ".intgf"));
                saveDialog.InitialDirectory = ExportConsole.Text;

                if (saveDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    using (StreamWriter writer = new StreamWriter(saveDialog.FileName))
                    {
                        string selectedDirectory = ExportConsole.Text;
                        string[] files = Directory.GetFiles(selectedDirectory, "*", SearchOption.AllDirectories);

                        string[] excludedExtensions = ExclusionRules.Text.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(e => "." + e.Trim().ToLower())
                                                        .ToArray();

                        ResultConsole.Document.Blocks.Clear();
                        long totalSize = 0;
                        int totalFiles = 0;
                        foreach (string file in files)
                        {
                            string fileExtension = System.IO.Path.GetExtension(file).ToLower();
                            if (!excludedExtensions.Contains(fileExtension))
                            {
                                FileInfo fileInfo = new FileInfo(file);
                                string relativePath = file.Substring(selectedDirectory.Length + 1);
                                writer.WriteLine($"{relativePath}\t{fileInfo.Length}");
                                totalSize += fileInfo.Length;
                                totalFiles++;
                            }
                        }
                        Paragraph pf = new Paragraph();
                        Run rf = new Run($"▰▰▰▰▰▰▰▰▰▰▰ Export completed ▰▰▰▰▰▰▰▰▰▰▰");
                        rf.Foreground = new SolidColorBrush(Colors.Green);
                        pf.Inlines.Add(rf);
                        ResultConsole.Document.Blocks.Add(pf);

                        Paragraph pt = new Paragraph();
                        Run rt = new Run($"Total files: {totalFiles} | Total size: {totalSize / 1024.0:F2} KB)");
                        rt.Foreground = new SolidColorBrush(Colors.White);
                        pt.Inlines.Add(rt);
                        ResultConsole.Document.Blocks.Add(pt);

                        Paragraph ps = new Paragraph();
                        Run rs = new Run($"File saved at: " + ExportConsole.Text);
                        rs.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x79, 0xD3, 0xF1));
                        ps.Inlines.Add(rt);
                        ResultConsole.Document.Blocks.Add(ps);
                        CenterConsole();
                    }
                    savedLogFilePath = saveDialog.FileName;
                }
            }
            catch (Exception ex)
            { System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }


        private void ImportTargetForVerification(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "INTGF Files (*.intgf)|*.intgf";
            openFileDialog.Title = "Select an INTGF file for verification";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                ImportConsole.Text = openFileDialog.FileName;
                fileSelected = true;
                EnableCheckIntegrity();
            }
        }

        private void CheckFileIntegrity(object sender, MouseButtonEventArgs e)
        {
            mismatchedFiles = new string[0];
            excessFiles = new string[0];
            missingFiles = new string[0];

            try
            {
                Dictionary<string, long> intgfFiles = new Dictionary<string, long>();
                using (StreamReader reader = new StreamReader(ImportConsole.Text))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] parts = line.Split('\t');
                        if (parts.Length == 2)
                        {
                            string filePath = parts[0];
                            long fileSize = long.Parse(parts[1]);
                            intgfFiles[filePath] = fileSize;
                        }
                    }
                }

                CommonSaveFileDialog saveDialog = new CommonSaveFileDialog();
                saveDialog.Title = "Save verification log";
                saveDialog.DefaultExtension = ".log";
                saveDialog.Filters.Add(new CommonFileDialogFilter("Text Files", ".log"));

                if (saveDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    List<string> excessFilesList = new List<string>();
                    List<string> mismatchFilesList = new List<string>();
                    List<string> missingFilesList = new List<string>();

                    using (StreamWriter writer = new StreamWriter(saveDialog.FileName))
                    {
                        int missingFiles = 0;
                        int sizeMatches = 0;
                        int sizeMismatches = 0;
                        int excessFiles = 0;

                        string[] excludedExtensions = ExclusionRules.Text.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(e => "." + e.Trim().ToLower())
                                                        .ToArray();

                        foreach (var kvp in intgfFiles)
                        {
                            string filePath = kvp.Key;
                            long expectedSize = kvp.Value;
                            string fullPath = System.IO.Path.Combine(TargetConsole.Text, filePath);
                            string fileExtension = System.IO.Path.GetExtension(filePath).ToLower();
                            if (excludedExtensions.Contains(fileExtension))
                            {
                                continue;
                            }
                            if (!File.Exists(fullPath))
                            {
                                writer.WriteLine($"Missing file: {filePath}");
                                missingFilesList.Add(filePath);
                                missingFiles++;
                            }
                            else
                            {
                                FileInfo fileInfo = new FileInfo(fullPath);
                                if (fileInfo.Length == expectedSize)
                                {
                                    sizeMatches++;
                                }
                                else
                                {
                                    writer.WriteLine($"Size mismatch: {filePath} (expected {expectedSize}, actual {fileInfo.Length})");
                                    mismatchFilesList.Add(filePath);
                                    sizeMismatches++;
                                }
                            }
                        }

                        string[] targetFiles = Directory.GetFiles(TargetConsole.Text, "*", SearchOption.AllDirectories);
                        foreach (string targetFile in targetFiles)
                        {
                            string relativeFilePath = targetFile.Substring(TargetConsole.Text.Length + 1);
                            string fileExtension = System.IO.Path.GetExtension(relativeFilePath).ToLower();
                            if (excludedExtensions.Contains(fileExtension))
                            {
                                continue;
                            }
                            if (!intgfFiles.ContainsKey(relativeFilePath) && !fileExtension.EndsWith(".intgf") && !fileExtension.EndsWith(".log"))
                            {
                                writer.WriteLine($"Excess file: {relativeFilePath}");
                                excessFilesList.Add(relativeFilePath);
                                excessFiles++;
                            }
                        }

                        ResultConsole.Document.Blocks.Clear();

                        Paragraph p = new Paragraph();
                        Run r = new Run($"▰▰▰▰▰▰▰▰▰▰ Verification completed ▰▰▰▰▰▰▰▰▰▰");

                        if (missingFiles == 0 && sizeMismatches == 0 && excessFiles == 0)
                        {
                            r.Foreground = new SolidColorBrush(Colors.Green);
                            writer.WriteLine($"Missing file: ");
                        }
                        else
                        {
                            r.Foreground = new SolidColorBrush(Colors.Red);
                        }

                        p.Inlines.Add(r);
                        ResultConsole.Document.Blocks.Add(p);

                        Paragraph pt = new Paragraph();
                        Run rt = new Run($"Missing files: {missingFiles} | Size mismatches: {sizeMismatches} | Excess files: {excessFiles}");
                        rt.Foreground = new SolidColorBrush(Colors.White);
                        pt.Inlines.Add(rt);
                        ResultConsole.Document.Blocks.Add(pt);

                        Paragraph ps = new Paragraph();
                        Run rs = new Run($"Verification log saved at: {saveDialog.FileName}");
                        rs.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x79, 0xD3, 0xF1));
                        ps.Inlines.Add(rs);
                        ResultConsole.Document.Blocks.Add(ps);

                        CenterConsole();

                        savedLogFilePath = saveDialog.FileName;

                    }

                    mismatchedFiles = mismatchFilesList.ToArray();
                    excessFiles = excessFilesList.ToArray();
                    if (mismatchedFiles.Length > 0 || excessFiles.Length > 0)
                    {
                        CopyFilesBtn.IsEnabled = true;
                        CopyFilesBtn.Source = enabledButtonImage;
                    }
                    OpenLastLogsBtn.IsEnabled = true;
                    OpenLastLogsBtn.Source = enabledButtonImage;

                }
            }
            catch (Exception ex)
            { System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void CenterConsole()
        {
            FlowDocument document = ResultConsole.Document;
            IEnumerable<Paragraph> paragraphs = document.Blocks.OfType<Paragraph>();
            foreach (Paragraph paragraph in paragraphs)
            { paragraph.TextAlignment = TextAlignment.Center; }
        }

        private void TargetDirectoryForComparison_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog();
            folderDialog.IsFolderPicker = true;
            folderDialog.Title = "Select the target folder for com";

            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                TargetConsole.Text = folderDialog.FileName;
                directorySelected = true;
                EnableCheckIntegrity();
            }
        }

        private void OpenLastLogs_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            { System.Diagnostics.Process.Start("notepad.exe", savedLogFilePath); }
            catch (Exception ex)
            { System.Windows.MessageBox.Show($"Error opening log file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void CopyFiles_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CommonOpenFileDialog saveDialog = new CommonOpenFileDialog();
            saveDialog.IsFolderPicker = true;
            saveDialog.Title = "Select a directory to copy the files to";

            if (saveDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string targetDirectory = saveDialog.FileName;

                try
                {
                    CopyFilesWithSubfolders(excessFiles, targetDirectory);
                    CopyFilesWithSubfolders(mismatchedFiles, targetDirectory);

                    foreach (string excessFile in excessFiles)
                    {
                        string srcPath = System.IO.Path.Combine(TargetConsole.Text, excessFile);
                        string dstPath = System.IO.Path.Combine(targetDirectory, excessFile);
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dstPath));
                        File.Copy(srcPath, dstPath, true);
                    }

                    System.Windows.MessageBox.Show($"Files have been copied to: {targetDirectory}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    System.Diagnostics.Process.Start("explorer.exe", targetDirectory);
                }
                catch (Exception ex)
                { System.Windows.MessageBox.Show($"An error occurred while copying files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }
        private void CopyFilesWithSubfolders(string[] filePaths, string targetDirectory)
        {
            foreach (string filePath in filePaths)
            {
                string srcPath = System.IO.Path.Combine(TargetConsole.Text, filePath);
                string dstPath = System.IO.Path.Combine(targetDirectory, filePath);
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dstPath));
                File.Copy(srcPath, dstPath, true);
            }
        }

        // Button Highlight
        private void Return_MouseEnter(object sender, MouseEventArgs e)
        { Return.Source = buttonH; }

        private void Return_MouseLeave(object sender, MouseEventArgs e)
        { Return.Source = buttonD; }
        private void Discord_MouseEnter(object sender, MouseEventArgs e)
        { Discord.Source = discordH; }

        private void Discord_MouseLeave(object sender, MouseEventArgs e)
        { Discord.Source = discordD; }

        private void Patreon_MouseEnter(object sender, MouseEventArgs e)
        { Patreon.Source = patreonH; }

        private void Patreon_MouseLeave(object sender, MouseEventArgs e)
        { Patreon.Source = patreonD; }

        private void Github_MouseEnter(object sender, MouseEventArgs e)
        { Github.Source = githubH; }

        private void Github_MouseLeave(object sender, MouseEventArgs e)
        { Github.Source = githubD; }

        private void Website_MouseEnter(object sender, MouseEventArgs e)
        { Website.Source = webH; }

        private void Website_MouseLeave(object sender, MouseEventArgs e)
        { Website.Source = webD; }
        private void DeviantArt_MouseEnter(object sender, MouseEventArgs e)
        { DeviantArt.Source = deviantArtH; }

        private void DeviantArt_MouseLeave(object sender, MouseEventArgs e)
        { DeviantArt.Source = deviantArtD; }

        private void Artstation_MouseEnter(object sender, MouseEventArgs e)
        { Artstation.Source = artstationH; }

        private void Artstation_MouseLeave(object sender, MouseEventArgs e)
        { Artstation.Source = artstationD; }


        public async Task Typewriter(int message, CancellationToken cancellationToken)
        {
            Author.Text = "";
            string msg;
            if (message == 0) { msg = ""; }
            else if (message == 1) { msg = "Created by Ms. Dysphoria"; }
            else { msg = "Discord: msdysphoria"; }

            Random randomDelay = new Random();

            for (int i = 0; i < msg.Length; i++)
            {
                // Check if the task has been canceled
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                Author.Text += msg[i].ToString();
                int delay = randomDelay.Next(35, 55);
                await Task.Delay(delay, cancellationToken);
            }

            if (message == 0)
            {
                await Task.Delay(2500, cancellationToken);
                await Typewriter(1, cancellationToken);
            }
            else if (message == 1)
            {
                Storyboard fadeInStoryboard = this.FindResource("GlowAuthor") as Storyboard;
                fadeInStoryboard.Begin();
                await Task.Delay(5000, cancellationToken);
                await Typewriter(2, cancellationToken);
            }
            else
            {

                await Task.Delay(5000, cancellationToken);
                await Typewriter(1, cancellationToken);
            }
        }

        // Links
        private void Patreon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string patreonLink = "https://www.patreon.com/msdysphoria";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = patreonLink,
                UseShellExecute = true
            });
        }

        private void Website_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string patreonLink = "https://msdysphoria.shop/";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = patreonLink,
                UseShellExecute = true
            });
        }

        private void DeviantArt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string patreonLink = "https://www.deviantart.com/msdysphoria";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = patreonLink,
                UseShellExecute = true
            });
        }

        private void Artstation_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string patreonLink = "https://www.artstation.com/msdysphoria";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = patreonLink,
                UseShellExecute = true
            });
        }

        private void Discord_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string patreonLink = "https://discord.gg/uQDPFt6WKn";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = patreonLink,
                UseShellExecute = true
            });
        }

        private void Github_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string patreonLink = "https://github.com/MsDysphoria";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = patreonLink,
                UseShellExecute = true
            });
        }

        private void Return_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            Storyboard fadeOutStoryboard = this.FindResource("FadeOut_Info") as Storyboard;
            fadeOutStoryboard?.Begin();
        }

        private void ShowInformation()
        {

            cancellationTokenSource = new CancellationTokenSource();
            _ = Typewriter(0, cancellationTokenSource.Token);

            Storyboard fadeInStoryboard = this.FindResource("FadeIn_Info") as Storyboard;
            fadeInStoryboard.Begin();
        }

    }
}
