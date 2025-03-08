using LanguageExt;
using LanguageExt.Common;
using MediaOrganiser.Services;
using MusicTools.Logic;
using SortPhotos.Core;
using System.Diagnostics;
using static SortPhotos.Core.Types;

namespace MediaOrganiser
{
    public partial class Form1 : Form
    {
        private readonly MediaService mediaService;
        private Image? currentImage;

        private readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MediaOrganiser",
            "settings.json");

        public Form1()
        {
            InitializeComponent();
            mediaService = new MediaService();

            // Subscribe to state changes
            ObservableState.StateChanged += OnStateChanged;
        }

        /// <summary>
        /// Updates UI components based on application state changes
        /// </summary>
        void OnStateChanged(object? sender, AppModel state)
        {
            if (InvokeRequired)
            {
                Invoke(() => OnStateChanged(sender, state));
                return;
            }

            // Update folder path display
            state.CurrentFolder.Match(
                Some: folder => txtFolderPath.Text = folder.Value,
                None: () => txtFolderPath.Text = string.Empty
            );

            // Update UI based on WorkInProgress state and folder availability
            btnScanFiles.Enabled = !state.WorkInProgress && state.CurrentFolder.IsSome;
            btnBrowseFolder.Enabled = !state.WorkInProgress;
            btnBin.Enabled = !state.WorkInProgress && state.Files.Count > 0;
            btnKeep.Enabled = !state.WorkInProgress && state.Files.Count > 0;
            btnPrevious.Enabled = !state.WorkInProgress && state.Files.Count > 0 && state.CurrentFile.Map(i => i.Value).IfNone(0) > 1;

            // Update progress bar
            progressScan.Style = state.WorkInProgress ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
            progressScan.MarqueeAnimationSpeed = state.WorkInProgress ? 30 : 0;
            if (!state.WorkInProgress) progressScan.Value = 0;

            // Update image display and navigation controls
            UpdateImageDisplay(state);
        }

        /// <summary>
        /// Updates the displayed image based on the current file selection
        /// </summary>
        void UpdateImageDisplay(AppModel state)
        {
            // Clear any existing image
            if (currentImage != null)
            {
                currentImage.Dispose();
                currentImage = null;
                picCurrentImage.Image = null;
            }

            // If we have a current file, try to load and display it
            state.CurrentFile.Match(
                Some: fileId =>
                {
                    if (state.Files.ContainsKey(fileId))
                    {
                        var mediaInfo = state.Files[fileId];

                        // Only attempt to load image files
                        if (mediaInfo.Category == FileCategory.Image)
                        {
                            try
                            {
                                // Load the image
                                currentImage = Image.FromFile(mediaInfo.FullPath.Value);
                                picCurrentImage.Image = currentImage;

                                // Update status with file info
                                var fileStatus = mediaInfo.State switch
                                {
                                    FileState.Keep => "Keep",
                                    FileState.Bin => "Bin",
                                    _ => "Unprocessed"
                                };

                                UpdateStatus($"Current file: {mediaInfo.FileName.Value} - {fileStatus}");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error loading image: {ex.Message}");
                                UpdateStatus($"Error loading image: {mediaInfo.FileName.Value}");
                            }
                        }
                        else if (mediaInfo.Category == FileCategory.Video)
                        {
                            // Show placeholder for videos
                            UpdateStatus($"Current file: {mediaInfo.FileName.Value} (Video) - cannot display preview");
                        }
                    }
                },
                None: () => UpdateStatus("No files selected")
            );
        }

        private void Form1_Load(object sender, EventArgs e) => LoadLastDirectory();

        /// <summary>
        /// Clean up resources and unsubscribe from events
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unsubscribe from state change events
                ObservableState.StateChanged -= OnStateChanged;

                // Dispose of any loaded image
                if (currentImage != null)
                {
                    currentImage.Dispose();
                    currentImage = null;
                }
            }

            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

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
                        ObservableState.SetCurrentFolder(lastPath);
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
            ObservableState.Current.CurrentFolder.Match(
                Some: folder => folderBrowserDialog.InitialDirectory = folder.Value,
                None: () => { } // Do nothing if no folder is set
            );

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                // Update the state with the new folder path
                ObservableState.SetCurrentFolder(folderBrowserDialog.SelectedPath);

                // Save the selected path for next time
                SaveLastDirectory(folderBrowserDialog.SelectedPath);
            }
        }

        private async void btnScanFiles_Click(object sender, EventArgs e) =>
            await Task.Run(() =>
                ObservableState.Current.CurrentFolder.Match(
                    Some: async path => await ScanDirectoryAsync(path.Value),
                    None: () => MessageBox.Show("Please select a folder first.", "No folder selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning)));

        async Task ScanDirectoryAsync(string path)
        {
            try
            {
                // Update state to show scanning in progress
                ObservableState.SetWorkInProgress(true);
                UpdateStatus("Status: Scanning files..."); // Use helper method instead of direct access

                // Perform the scan
                var result = await mediaService.ScanDirectoryAsync(path);

                // Process the result
                result.Match(
                    Right: fileResponse =>
                    {
                        // Handle success
                        var fileCount = fileResponse.Files.Length;
                        UpdateStatus($"Status: Found {fileCount} media files."); // Use helper method

                        if (fileResponse.UserErrors.Length > 0)
                        {
                            var errorMessages = string.Join(Environment.NewLine,
                                fileResponse.UserErrors.Select(ue => ue.message));

                            ShowMessageBox($"Scan completed with some warnings:{Environment.NewLine}{errorMessages}",
                                "Scan Warnings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    },
                    Left: error =>
                    {
                        // Handle error
                        UpdateStatus("Status: Error during scan."); // Use helper method
                        ShowMessageBox($"An error occurred while scanning: {error.Message}",
                            "Scan Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Debug.WriteLine($"Error details: {error}");
                    });
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions
                UpdateStatus("Status: Error during scan."); // Use helper method
                ShowMessageBox($"An unexpected error occurred: {ex.Message}",
                    "Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Exception details: {ex}");
            }
            finally
            {
                // Reset the work in progress state
                ObservableState.SetWorkInProgress(false);
            }
        }

        /// <summary>
        /// Handles the Previous button click
        /// </summary>
        private void btnPrevious_Click(object sender, EventArgs e) => ObservableState.PreviousFile();

        /// <summary>
        /// Handles the Bin button click
        /// </summary>
        private void btnBin_Click(object sender, EventArgs e) => ObservableState.UpdateFileState(FileState.Bin);

        /// <summary>
        /// Handles the Keep button click
        /// </summary>
        private void btnKeep_Click(object sender, EventArgs e) => ObservableState.UpdateFileState(FileState.Keep);

        // Helper method to safely update status label from any thread
        void UpdateStatus(string text)
        {
            if (lblStatus.InvokeRequired)
                lblStatus.Invoke(() => lblStatus.Text = text);
            else
                lblStatus.Text = text;
        }

        // Helper method to safely show message box from any thread
        void ShowMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            if (InvokeRequired)
                Invoke(() => MessageBox.Show(this, message, caption, buttons, icon));
            else
                MessageBox.Show(this, message, caption, buttons, icon);
        }
    }
}