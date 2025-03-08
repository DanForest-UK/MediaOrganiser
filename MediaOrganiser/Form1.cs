using LanguageExt;
using LanguageExt.Common;
using MediaOrganiser.Services;
using MusicTools.Logic;
using SortPhotos.Core;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using static SortPhotos.Core.Types;

namespace MediaOrganiser
{
    public partial class Form1 : Form
    {
        readonly MediaService mediaService;
        Image? currentImage;

        // Modern video player control
        VideoPlayerControl? videoPlayer;

        readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MediaOrganiser",
            "settings.json");

        public Form1()
        {
            InitializeComponent();
            mediaService = new MediaService();

            // Subscribe to state changes and initialize video player
            ObservableState.StateChanged += OnStateChanged;
            InitializeVideoPlayer();
        }

        /// <summary>
        /// Initializes the video player control
        /// </summary>
        void InitializeVideoPlayer()
        {
            try
            {
                videoPlayer = new VideoPlayerControl();
                videoPlayer.Dock = DockStyle.Fill;
                videoPlayer.Visible = false;
                picCurrentImage.Controls.Add(videoPlayer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing video player: {ex.Message}");
                UpdateStatus("Error: Video player could not be initialized");
            }
        }

        /// <summary>
        /// Updates UI components based on application state changes
        /// </summary>
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

            // Enable Organise Files button if we have files with Keep or Bin state and not working
            var hasFilesToOrganise = !state.WorkInProgress &&
                state.Files.Values.Any(f => f.State == FileState.Keep || f.State == FileState.Bin);
            btnOrganiseFiles.Enabled = hasFilesToOrganise;

            // Update progress bar
            progressScan.Style = state.WorkInProgress ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
            progressScan.MarqueeAnimationSpeed = state.WorkInProgress ? 30 : 0;
            if (!state.WorkInProgress) progressScan.Value = 0;

            // Update image display and navigation controls
            UpdateMediaDisplay(state);
        }

        /// <summary>
        /// Updates the displayed media based on the current file selection
        /// </summary>
        void UpdateMediaDisplay(AppModel state)
        {
            // Stop any playing media
            StopMedia();

            // If we have a current file, try to load and display it
            state.CurrentFile.Match(
                Some: fileId => {
                    if (!state.Files.ContainsKey(fileId)) return;

                    var mediaInfo = state.Files[fileId];
                    var fileStatus = mediaInfo.State switch
                    {
                        FileState.Keep => "Keep",
                        FileState.Bin => "Bin",
                        _ => "Unprocessed"
                    };

                    var fileSizeText = FormatFileSize(mediaInfo.Size.Value);

                    if (mediaInfo.Category == FileCategory.Image)
                    {
                        DisplayImage(mediaInfo);
                        UpdateStatus($"Current file: {mediaInfo.FileName.Value} ({fileSizeText}) - {fileStatus}");
                    }
                    else if (mediaInfo.Category == FileCategory.Video)
                    {
                        DisplayVideo(mediaInfo);
                        UpdateStatus($"Current file: {mediaInfo.FileName.Value} (Video, {fileSizeText}) - {fileStatus}");
                    }
                },
                None: () => UpdateStatus("No files selected")
            );
        }



        /// <summary>
        /// Formats file size as KB or MB based on size
        /// </summary>
        string FormatFileSize(long bytes) =>
            bytes < 1024 * 1024
                ? $"{bytes / 1024.0:F1} KB"
                : $"{bytes / (1024.0 * 1024.0):F2} MB";

        /// <summary>
        /// Displays an image file
        /// </summary>
        void DisplayImage(MediaInfo mediaInfo)
        {
            try
            {
                // Hide video player, show picture box
                if (videoPlayer != null) videoPlayer.Visible = false;
                picCurrentImage.Visible = true;

                // Dispose current image if exists
                if (currentImage != null)
                {
                    currentImage.Dispose();
                    currentImage = null;
                }

                // Load and display the new image
                currentImage = Image.FromFile(mediaInfo.FullPath.Value);
                picCurrentImage.Image = currentImage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading image: {ex.Message}");
                UpdateStatus($"Error loading image: {mediaInfo.FileName.Value}");
            }
        }

        /// <summary>
        /// Displays and plays a video file
        /// </summary>
        void DisplayVideo(MediaInfo mediaInfo)
        {
            try
            {
                // Hide picture box image
                picCurrentImage.Image = null;

                // Play the video if video player exists
                if (videoPlayer != null)
                {
                    videoPlayer.Visible = true;
                    videoPlayer.SetSource(mediaInfo.FullPath.Value);
                    videoPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing video: {ex.Message}");
                UpdateStatus($"Error playing video: {mediaInfo.FileName.Value}");
            }
        }

        /// <summary>
        /// Stops any currently playing media
        /// </summary>
        void StopMedia()
        {
            // Stop any playing video
            if (videoPlayer != null && videoPlayer.Visible)
                try
                {
                    videoPlayer.Stop();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error stopping media: {ex.Message}");
                }

            // Clear any loaded image
            if (currentImage != null)
            {
                currentImage.Dispose();
                currentImage = null;
                picCurrentImage.Image = null;
            }
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

                // Clean up video player
                if (videoPlayer != null)
                {
                    try
                    {
                        videoPlayer.Stop();
                        videoPlayer.Dispose();
                        videoPlayer = null;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error disposing video player: {ex.Message}");
                    }
                }
            }

            if (disposing && (components != null))
                components.Dispose();

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
                    None: () => MessageBox.Show("Please select a folder first.",
                                               "No folder selected",
                                               MessageBoxButtons.OK,
                                               MessageBoxIcon.Warning)));

        async Task ScanDirectoryAsync(string path)
        {
            try
            {
                // Update state to show scanning in progress
                ObservableState.SetWorkInProgress(true);
                UpdateStatus("Status: Scanning files...");

                // Perform the scan
                var result = await mediaService.ScanDirectoryAsync(path);

                // Process the result
                result.Match(
                    Right: fileResponse => {
                        // Handle success
                        var fileCount = fileResponse.Files.Length;
                        UpdateStatus($"Status: Found {fileCount} media files.");

                        if (fileResponse.UserErrors.Length > 0)
                        {
                            var errorMessages = string.Join(Environment.NewLine,
                                fileResponse.UserErrors.Select(ue => ue.message));

                            ShowMessageBox($"Scan completed with some warnings:{Environment.NewLine}{errorMessages}",
                                "Scan Warnings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    },
                    Left: error => {
                        // Handle error
                        UpdateStatus("Status: Error during scan.");
                        ShowMessageBox($"An error occurred while scanning: {error.Message}",
                            "Scan Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Debug.WriteLine($"Error details: {error}");
                    });
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions
                UpdateStatus("Status: Error during scan.");
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
        /// Handles the Organise Files button click
        /// </summary>
        private async void btnOrganiseFiles_Click(object sender, EventArgs e)
        {
            // Only proceed if we have files to organise
            var files = ObservableState.Current.Files.Values.Where(f => f.State != FileState.Unprocessed);
            var fileCount = files.Count();

            if (fileCount == 0)
            {
                ShowMessageBox("No files marked for keeping or deletion.",
                    "No Files to Organise", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Ask for confirmation before proceeding
            var binCount = mediaService.CountFilesForDeletion();
            var keepCount = fileCount - binCount;

            var confirmMessage = $"You are about to:{Environment.NewLine}{Environment.NewLine}" +
                $"• Organise {keepCount} file(s) marked for keeping{Environment.NewLine}" +
                $"• Delete {binCount} file(s) marked for deletion{Environment.NewLine}{Environment.NewLine}" +
                "Do you want to continue?";

            var confirmResult = MessageBox.Show(this, confirmMessage,
                "Confirm Organisation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes)
                return;

            // Get destination folder
            var destinationDialog = new FolderBrowserDialog
            {
                Description = "Select destination folder for organised files",
                UseDescriptionForTitle = true
            };

            if (destinationDialog.ShowDialog() != DialogResult.OK)
                return;

            var destinationPath = destinationDialog.SelectedPath;

            try
            {
                // Update state to show work in progress
                ObservableState.SetWorkInProgress(true);
                UpdateStatus("Status: Organising files...");

                // Call the organize method
                var result = await mediaService.OrganizeFilesAsync(destinationPath);

                // Process the result
                result.Match(
                    Right: count => {
                        UpdateStatus($"Status: Successfully organised {count} files.");
                        ShowMessageBox($"Successfully organised {count} files.",
                            "Organisation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    },
                    Left: error => {
                        UpdateStatus("Status: Error during file organisation.");
                        ShowMessageBox($"An error occurred: {error.Message}",
                            "Organisation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Debug.WriteLine($"Error details: {error}");
                    });
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions
                UpdateStatus("Status: Unexpected error during file organisation.");
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

        // Helper methods for thread-safe UI updates

        // Helper methods for thread-safe UI updates

        void UpdateStatus(string text)
        {
            if (lblStatus.InvokeRequired)
                lblStatus.Invoke(() => lblStatus.Text = text);
            else
                lblStatus.Text = text;
        }

        void ShowMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            if (InvokeRequired)
                Invoke(() => MessageBox.Show(this, message, caption, buttons, icon));
            else
                MessageBox.Show(this, message, caption, buttons, icon);
        }
    }
}