using System.Diagnostics;
using SortPhotos.UI;
using SortPhotos.Logic;
using static SortPhotos.Core.Types;

namespace SortPhotos.Forms
{
    public partial class MainForm : Form
    {
        private readonly MediaService _mediaService;
        private string _currentScanPath;

        public MainForm()
        {
            InitializeComponent();
            _mediaService = new MediaService();

            // Load default path from config
            _currentScanPath = ConfigManager.GetDefaultScanPath();
            txtScanPath.Text = _currentScanPath;
        }

        private async void btnScan_Click(object sender, EventArgs e)
        {
            // Show loading indicator
            progressBar.Visible = true;
            lblStatus.Text = "Scanning files...";
            panelItems.Controls.Clear();

            try
            {
                var result = await _mediaService.ScanDirectoryAsync(_currentScanPath);

                result.Match(
                    Right: response =>
                    {
                        DisplayFiles(response.Files);
                        lblStatus.Text = $"Found {response.Files.Count()} files";
                        // todo display user errors
                    },
                    Left: error =>
                    {
                        MessageBox.Show($"Error scanning files: {error.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        lblStatus.Text = "Error scanning files";
                    }
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error";
            }
            finally
            {
                progressBar.Visible = false;
            }
        }

        private void DisplayFiles(IEnumerable<MediaInfo> files)
        {
            panelItems.Controls.Clear();
            int yPos = 10;

            foreach (var file in files)
            {
                var itemControl = new MediaItemControl(file)
                {
                    Location = new System.Drawing.Point(10, yPos)
                };

                // Subscribe to events
                itemControl.KeepClicked += OnKeepClicked;
                itemControl.BinClicked += OnBinClicked;
                itemControl.PlayClicked += OnPlayClicked;

                panelItems.Controls.Add(itemControl);
                yPos += itemControl.Height + 5;
            }
        }

        private void OnKeepClicked(object sender, MediaInfo fileInfo)
        {
            _mediaService.UpdateFileState(fileInfo.FullPath, FileState.Keep);
            panelItems.Controls.Remove((Control)sender);
            RefreshFileCountStatus();
        }

        private void OnBinClicked(object sender, MediaInfo fileInfo)
        {
            _mediaService.UpdateFileState(fileInfo.FullPath, FileState.Bin);
            panelItems.Controls.Remove((Control)sender);
            RefreshFileCountStatus();
        }

        private void OnPlayClicked(object sender, MediaInfo fileInfo)
        {
            try
            {
                // Open the video file with the default player
                Process.Start(new ProcessStartInfo
                {
                    FileName = fileInfo.FullPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error playing video: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshFileCountStatus()
        {
            var displayedCount = _mediaService.GetDisplayedFiles().Count();
            var binCount = _mediaService.CountFilesForDeletion();
            var keepCount = _mediaService.GetFilesToProcess().Count(f => f.State == FileState.Keep);

            lblStatus.Text = $"Displayed: {displayedCount} | Keep: {keepCount} | Bin: {binCount}";
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = _currentScanPath;
                dialog.Description = "Select folder to scan";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _currentScanPath = dialog.SelectedPath;
                    txtScanPath.Text = _currentScanPath;

                    // Save to config
                    ConfigManager.SaveDefaultScanPath(_currentScanPath);
                }
            }
        }

        private async void btnOrganize_Click(object sender, EventArgs e)
        {
            int binCount = _mediaService.CountFilesForDeletion();
            int keepCount = _mediaService.GetFilesToProcess().Count(f => f.State == FileState.Keep);

            if (binCount == 0 && keepCount == 0)
            {
                MessageBox.Show("No files to organize.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string message = "You are about to:";
            if (binCount > 0) message += $"\n- Delete {binCount} files";
            if (keepCount > 0) message += $"\n- Organize {keepCount} files";
            message += "\n\nDo you want to continue?";

            if (MessageBox.Show(message, "Confirm Organization",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                progressBar.Visible = true;
                lblStatus.Text = "Organizing files...";

                try
                {
                    var result = await _mediaService.OrganizeFilesAsync(_currentScanPath);

                    result.Match(
                        Right: processedCount =>
                        {
                            MessageBox.Show($"Successfully processed {processedCount} files.",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            lblStatus.Text = "Organization complete";

                            // Refresh file list
                            btnScan_Click(sender, e);
                        },
                        Left: error =>
                        {
                            MessageBox.Show($"Error organizing files: {error.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            lblStatus.Text = "Error organizing files";
                        }
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unexpected error: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "Error";
                }
                finally
                {
                    progressBar.Visible = false;
                }
            }
        }
    }
}