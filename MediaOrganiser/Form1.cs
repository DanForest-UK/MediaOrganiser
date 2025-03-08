using LanguageExt.Common;
using MediaOrganiser.Services;
using SortPhotos.Core;
using System.Diagnostics;

namespace MediaOrganiser
{
    public partial class Form1 : Form
    {
        private readonly MediaService mediaService;

        private readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MediaOrganiser",
            "settings.json");

        public Form1()
        {
            InitializeComponent();
            mediaService = new MediaService();
        }

        private void Form1_Load(object sender, EventArgs e) => LoadLastDirectory();

        /// <summary>
        /// Loads the last used directory path from settings
        /// </summary>
        void LoadLastDirectory()
        {
            try
            {
                var settingsFolder = Path.GetDirectoryName(SettingsFilePath);

                if (!Directory.Exists(settingsFolder))
                    Directory.CreateDirectory(settingsFolder!);

                if (File.Exists(SettingsFilePath))
                {
                    var lastPath = File.ReadAllText(SettingsFilePath);
                    if (lastPath.HasValue() && Directory.Exists(lastPath))
                    {
                        txtFolderPath.Text = lastPath;
                        btnScanFiles.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading last directory: {ex.Message}");
                // Don't show error to user, just silently fail
            }
        }

        /// <summary>
        /// Saves the current directory path to settings
        /// </summary>
        void SaveLastDirectory(string path)
        {
            try
            {
                var settingsFolder = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(settingsFolder))
                    Directory.CreateDirectory(settingsFolder!);

                File.WriteAllText(SettingsFilePath, path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving last directory: {ex.Message}");
                // Don't show error to user, just silently fail
            }
        }

        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            // Set initial folder if we have one
            if (!string.IsNullOrEmpty(txtFolderPath.Text) && Directory.Exists(txtFolderPath.Text))
                folderBrowserDialog.InitialDirectory = txtFolderPath.Text;

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtFolderPath.Text = folderBrowserDialog.SelectedPath;
                btnScanFiles.Enabled = !string.IsNullOrEmpty(txtFolderPath.Text);

                // Save the selected path for next time
                SaveLastDirectory(folderBrowserDialog.SelectedPath);
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

        /// <summary>
        /// Performs an asynchronous scan of the specified directory
        /// </summary>
        async Task ScanDirectoryAsync(string path)
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
                var result = await mediaService.ScanDirectoryAsync(path);

                // Process the result
                result.Match(
                    Right: fileResponse =>
                    {
                        // Handle success
                        var fileCount = fileResponse.Files.Count();
                        lblStatus.Text = $"Status: Found {fileCount} media files.";

                        if (fileResponse.UserErrors.Count() > 0)
                        {
                            var errorMessages = string.Join(Environment.NewLine,
                                fileResponse.UserErrors.Select(ue => ue.message));

                            MessageBox.Show($"Scan completed with some warnings:{Environment.NewLine}{errorMessages}",
                                "Scan Warnings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    },
                    Left: error =>
                    {
                        // Handle error
                        lblStatus.Text = "Status: Error during scan.";
                        MessageBox.Show($"An error occurred while scanning: {error.Message}",
                            "Scan Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Debug.WriteLine($"Error details: {error}");
                    });
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions
                lblStatus.Text = "Status: Error during scan.";
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