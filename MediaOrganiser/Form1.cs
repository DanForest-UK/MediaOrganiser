using LanguageExt.Common;
using MediaOrganiser.Services;
using System.Diagnostics;

namespace MediaOrganiser
{
    public partial class Form1 : Form
    {
        private readonly MediaService _mediaService;

        public Form1()
        {
            InitializeComponent();
            _mediaService = new MediaService();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Form initialization if needed
        }

        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtFolderPath.Text = folderBrowserDialog.SelectedPath;
                btnScanFiles.Enabled = !string.IsNullOrEmpty(txtFolderPath.Text);
            }
        }

        private async void btnScanFiles_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFolderPath.Text))
            {
                MessageBox.Show("Please select a folder first.", "No folder selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await ScanDirectoryAsync(txtFolderPath.Text);
        }

        private async Task ScanDirectoryAsync(string path)
        {
            try
            {
                // Update UI to show scanning in progress
                btnScanFiles.Enabled = false;
                btnBrowseFolder.Enabled = false;
                lblStatus.Text = "Status: Scanning files...";
                progressScan.Style = ProgressBarStyle.Marquee;
                progressScan.MarqueeAnimationSpeed = 30;

                // Perform the scan
                var result = await _mediaService.ScanDirectoryAsync(path);

                // Process the result
                result.Match(
                    Right: fileResponse =>
                    {
                        // Handle success
                        var fileCount = fileResponse.Files.Count();
                        lblStatus.Text = $"Status: Found {fileCount} media files.";

                        if (fileResponse.UserErrors.Count() > 0)
                        {
                            // Show warnings if there were user errors
                            var errorMessages = string.Join(Environment.NewLine,
                                fileResponse.UserErrors.Select(ue => ue.message));

                            MessageBox.Show($"Scan completed with some warnings:{Environment.NewLine}{errorMessages}",
                                "Scan Warnings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                        return fileResponse;
                    },
                    Left: error =>
                    {
                        // Handle error
                        lblStatus.Text = $"Status: Error during scan.";
                        MessageBox.Show($"An error occurred while scanning: {error.Message}",
                            "Scan Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Debug.WriteLine($"Error details: {error}");
                        return null;
                    });
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions
                lblStatus.Text = $"Status: Error during scan.";
                MessageBox.Show($"An unexpected error occurred: {ex.Message}",
                    "Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Exception details: {ex}");
            }
            finally
            {
                // Re-enable the controls
                btnScanFiles.Enabled = !string.IsNullOrEmpty(txtFolderPath.Text);
                btnBrowseFolder.Enabled = true;
                progressScan.Style = ProgressBarStyle.Blocks;
                progressScan.MarqueeAnimationSpeed = 0;
                progressScan.Value = 0;
            }
        }
    }
}