using LanguageExt;
using LanguageExt.Common;
using MediaOrganiser.Services;
using MusicTools.Logic;
using MediaOrganiser.Core;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using static MediaOrganiser.Core.Types;
using static LanguageExt.Prelude;

namespace MediaOrganiser
{
    public partial class Form1 : Form
    {
        readonly MediaService mediaService;
        System.Drawing.Image? currentImage;

        // Media player controls
        VideoPlayerControl? videoPlayer;
        DocumentViewerControl? documentViewer;
        Panel? openFolderPanel;
        Button? openFolderButton;

        readonly string settingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MediaOrganiser",
            "settings.json");

        public Form1()
        {
            InitializeComponent();

            // Apply theme to the entire form
            ThemeManager.ApplyTheme(this);

            // Apply special styles to action buttons
            ThemeManager.StyleSuccessButton(btnKeep);
            ThemeManager.StyleDangerButton(btnBin);
            ThemeManager.StylePrimaryButton(btnScanFiles);
            ThemeManager.StylePrimaryButton(btnOrganiseFiles);
            ThemeManager.StylePrimaryButton(btnBrowseFolder);

            // Apply navigation button styles
            ThemeManager.StyleSecondaryButton(btnNext);
            ThemeManager.StyleSecondaryButton(btnPrevious);

            // Set form background color and appearance
            this.BackColor = ThemeManager.FormBackgroundColor;

            // Style the main picture display area
            picCurrentImage.BorderStyle = BorderStyle.FixedSingle;
            picCurrentImage.BackColor = ThemeManager.PrimaryBackColor;

            mediaService = new MediaService();

            ObservableState.StateChanged += OnStateChanged;
            InitializeVideoPlayer();
            InitializeDocumentViewer();

            // Add event handler for KeepParentFolder checkbox
            chkKeepParentFolder.CheckedChanged += chkKeepParentFolder_Changed;
        }

        /// <summary>
        /// Loads previous session or directory on form load
        /// </summary>
        void Form1_Load(object sender, EventArgs e)
        {
            LoadLastDirectory();
            CheckSavedStateAsync();
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
        /// Initializes the document viewer control
        /// </summary>
        void InitializeDocumentViewer()
        {
            try
            {
                documentViewer = new DocumentViewerControl();
                documentViewer.Dock = DockStyle.Fill;
                documentViewer.Visible = false;
                picCurrentImage.Controls.Add(documentViewer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing document viewer: {ex.Message}");
                UpdateStatus("Error: Document viewer could not be initialized");
            }
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

            state.CurrentFolder.Match(
                Some: folder => txtFolderPath.Text = folder.Value,
                None: () => txtFolderPath.Text = string.Empty);

            btnScanFiles.Enabled = !state.WorkInProgress && state.CurrentFolder.IsSome;
            btnBrowseFolder.Enabled = !state.WorkInProgress;


            btnBin.Enabled = !state.WorkInProgress && state.Files.Count > 0;
            btnKeep.Enabled = !state.WorkInProgress &&
                               state.Files.Count > 0;

            var currentFileIndex = state.CurrentFile.Map(i => i.Value).IfNone(0);
            btnPrevious.Enabled = !state.WorkInProgress && state.Files.Count > 0 &&
                                  currentFileIndex > 1 &&
                                  currentFileIndex <= state.Files.Count;

            btnNext.Enabled = !state.WorkInProgress &&
                              state.Files.Count > 0 &&
                              currentFileIndex < state.Files.Count;

            btnOrganiseFiles.Enabled = !state.WorkInProgress &&
                (from f in state.Files.Values
                 where f.State == FileState.Keep ||
                       f.State == FileState.Bin
                 select unit).Any();

            progressScan.Style = state.WorkInProgress ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
            progressScan.MarqueeAnimationSpeed = state.WorkInProgress ? 30 : 0;
            if (!state.WorkInProgress) progressScan.Value = 0;

            // Update CopyOnly checkbox
            chkCopyOnly.Checked = state.CopyOnly.Value;
            chkSortByYear.Checked = state.SortByYear.Value;
            chkKeepParentFolder.Checked = state.KeepParentFolder.Value; // Added this line for the new checkbox

            UpdateMediaDisplay(state);

            if (!state.WorkInProgress)
                StateSerialiser.SaveState(state);
        }

        /// <summary>
        /// Updates the displayed media based on the current file selection
        /// </summary>
        void UpdateMediaDisplay(AppModel state)
        {
            StopMedia();

            // Set visibility of the center controls panel
            pnlCenterControls.Visible = state.CurrentFile.IsSome;

            state.CurrentFile.Match(
                Some: fileId =>
                {
                    if (!state.Files.ContainsKey(fileId)) return;

                    var mediaInfo = state.Files[fileId];
                    var fileStatus = mediaInfo.State switch
                    {
                        FileState.Keep => "Keep", // todo can get straight from enum
                        FileState.Bin => "Bin",
                        _ => "Undecided"
                    };

                    tbFileName.Text = mediaInfo.FileName.Value;

                    var fileSizeText = FormatFileSize(mediaInfo.Size.Value);
                    var fullFileName = $"{mediaInfo.FileName.Value}.{mediaInfo.Extension.Value.TrimStart('.')}";

                    var lastFileText = state.Files.Count() <= state.CurrentFile.Map(v => v.Value).IfNone(0)
                        ? " : This is the final file"
                        : "";

                    var placeText = $"{state.CurrentFile.Map(v => v.Value).IfNone(0)} of {state.Files.Count()} files: ";

                    UpdateStatus($"{placeText}{fullFileName} ({fileSizeText}) - {fileStatus}{lastFileText}");

                    if (mediaInfo.Category == FileCategory.Image)
                        DisplayImage(mediaInfo);

                    else if (mediaInfo.Category == FileCategory.Video)
                        DisplayVideo(mediaInfo);

                    else if (mediaInfo.Category == FileCategory.Document)
                        DisplayDocument(mediaInfo);

                },
                None: () =>
                {
                    UpdateStatus("No files selected");
                    // Hide the center controls panel when no file is selected
                    pnlCenterControls.Visible = false;
                });
        }

        /// <summary>
        /// Displays a document file
        /// </summary>
        void DisplayDocument(MediaInfo mediaInfo)
        {
            try
            {
                // todo can all this visiblity be driven from the main state
                btnRotateLeft.Visible = false;
                btnRotateRight.Visible = false;
                if (videoPlayer != null) videoPlayer.Visible = false;
                picCurrentImage.Image = null;

                picCurrentImage.Visible = true;

                if (openFolderPanel != null) openFolderPanel.Visible = false;

                if (documentViewer != null)
                {
                    documentViewer.Visible = true;
                    documentViewer.BringToFront();

                    var extension = Path.GetExtension(mediaInfo.FullPath.Value).ToLower();
                    if (extension == ".doc" || extension == ".docx")
                        Debug.WriteLine($"Loading Word document: {mediaInfo.FullPath.Value}");
                    else
                        Debug.WriteLine($"Loading document: {mediaInfo.FullPath.Value}");

                    Debug.WriteLine($"Document viewer visible: {documentViewer.Visible}");
                    Debug.WriteLine($"Picture box visible: {picCurrentImage.Visible}");
                    Debug.WriteLine($"Document viewer size: {documentViewer.Width}x{documentViewer.Height}");

                    documentViewer.LoadDocument(mediaInfo.FullPath.Value);
                }
                else
                    Debug.WriteLine("Document viewer is null");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error displaying document: {ex.Message}");
                ShowErrorWithOpenFolderButton(mediaInfo);
            }
        }

        void btnRotateLeft_Click(object sender, EventArgs e) =>
            ObservableState.RotateCurrentImage(Rotation.Rotate270); // Counterclockwise

        void btnRotateRight_Click(object sender, EventArgs e) =>
            ObservableState.RotateCurrentImage(Rotation.Rotate90); // Clockwise

        /// <summary>
        /// Shows error message with button to open file's directory
        /// </summary>
        void ShowErrorWithOpenFolderButton(MediaInfo mediaInfo)
        {
            if (openFolderPanel == null)
            {
                openFolderPanel = new Panel
                {
                    Dock = DockStyle.Fill
                };
                ThemeManager.StylePanel(openFolderPanel, ThemeManager.SecondaryBackColor);

                var errorLabel = new Label
                {
                    Text = "Unable to preview file",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 40
                };
                ThemeManager.StyleLabel(errorLabel, ThemeManager.HeaderFont);

                openFolderButton = new Button
                {
                    Text = "Open enclosing folder",
                    Dock = DockStyle.Top,
                    Height = 40,
                    Width = 200
                };
                ThemeManager.StyleButton(openFolderButton);

                openFolderButton.Click += (s, e) =>
                {
                    var folderPath = Path.GetDirectoryName(mediaInfo.FullPath.Value);
                    if (Directory.Exists(folderPath))
                        Process.Start("explorer.exe", folderPath);
                };

                var containerPanel = new Panel
                {
                    Width = 200,
                    Height = 100,
                    Anchor = AnchorStyles.None
                };
                ThemeManager.StylePanel(containerPanel, ThemeManager.SecondaryBackColor);

                containerPanel.Controls.Add(openFolderButton);
                containerPanel.Controls.Add(errorLabel);
                containerPanel.Location = new System.Drawing.Point(
                    (openFolderPanel.Width - containerPanel.Width) / 2,
                    (openFolderPanel.Height - containerPanel.Height) / 2);

                openFolderPanel.Controls.Add(containerPanel);
                picCurrentImage.Controls.Add(openFolderPanel);
            }

            if (videoPlayer != null) videoPlayer.Visible = false;
            if (documentViewer != null) documentViewer.Visible = false;
            picCurrentImage.Visible = false;

            openFolderPanel.Visible = true;

            if (openFolderButton != null)
                openFolderButton.Tag = mediaInfo.FullPath.Value;
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
                // todo can all this visiblity be driven from the main state
                btnRotateLeft.Visible = true;
                btnRotateRight.Visible = true;
                if (videoPlayer != null) videoPlayer.Visible = false;
                if (documentViewer != null) documentViewer.Visible = false;
                if (openFolderPanel != null) openFolderPanel.Visible = false;
                picCurrentImage.Visible = true;

                if (currentImage != null)
                {
                    currentImage.Dispose();
                    currentImage = null;
                }

                // Load the original image
                currentImage = System.Drawing.Image.FromFile(mediaInfo.FullPath.Value);

                // Apply rotation if needed
                if (mediaInfo.Rotation != Rotation.None)
                {
                    RotateFlipType rotateFlip = mediaInfo.Rotation switch
                    {
                        Rotation.Rotate90 => RotateFlipType.Rotate90FlipNone,
                        Rotation.Rotate180 => RotateFlipType.Rotate180FlipNone,
                        Rotation.Rotate270 => RotateFlipType.Rotate270FlipNone,
                        _ => RotateFlipType.RotateNoneFlipNone
                    };

                    // Create a new bitmap with the rotation applied
                    System.Drawing.Bitmap rotatedImage = new System.Drawing.Bitmap(currentImage);
                    rotatedImage.RotateFlip(rotateFlip);

                    // Dispose the original and use the rotated one
                    currentImage.Dispose();
                    currentImage = rotatedImage;
                }

                picCurrentImage.Image = currentImage;

                // Show/hide rotation buttons for images only
                btnRotateLeft.Visible = true;
                btnRotateRight.Visible = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading image: {ex.Message}");
                ShowErrorWithOpenFolderButton(mediaInfo);
            }
        }

        /// <summary>
        /// Displays and plays a video file
        /// </summary>
        void DisplayVideo(MediaInfo mediaInfo)
        {
            try
            {
                // todo can all this visiblity be driven from the main state
                btnRotateLeft.Visible = false;
                btnRotateRight.Visible = false;
                picCurrentImage.Image = null;
                if (documentViewer != null) documentViewer.Visible = false;
                if (openFolderPanel != null) openFolderPanel.Visible = false;

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
                ShowErrorWithOpenFolderButton(mediaInfo);
            }
        }

        /// <summary>
        /// Stops any currently playing media or closes documents
        /// </summary>
        void StopMedia()
        {
            if (videoPlayer != null && videoPlayer.Visible)
                try
                {
                    videoPlayer.Stop();
                    videoPlayer.Visible = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error stopping media: {ex.Message}");
                }

            try
            {
                if (documentViewer != null)
                {
                    documentViewer.Dispose();
                    documentViewer = null;
                    InitializeDocumentViewer();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error resetting document viewer: {ex.Message}");
            }

            if (currentImage != null)
            {
                currentImage.Dispose();
                currentImage = null;
                picCurrentImage.Image = null;
            }
        }

        // If you specifically need to run CheckSavedState on a background thread,
        // use this version instead:
        void CheckSavedStateAsync()
        {
            Task.Run(() =>
            {
                var savedState = StateSerialiser.LoadState();

                if (savedState.Exists(s => s.Files.Keys.Any()))
                {
                    // Use BeginInvoke to marshal back to the UI thread
                    this.BeginInvoke(() =>
                    {
                        savedState.IfSome(state =>
                        {
                            var result = MessageBox.Show(
                                this,
                                "Would you like to continue your previous session?",
                                "Previous Session Found",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (result == DialogResult.Yes)
                            {
                                UpdateStatus("Previous session loaded");
                                ObservableState.Update(state);
                            }
                            else
                            {
                                StateSerialiser.DeleteState();
                            }
                        });
                    });
                }
            });
        }

        /// <summary>
        /// Clean up resources and unsubscribe from events
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ObservableState.StateChanged -= OnStateChanged;

                if (currentImage != null)
                {
                    currentImage.Dispose();
                    currentImage = null;
                }

                if (videoPlayer != null)
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

                if (documentViewer != null)
                    try
                    {
                        documentViewer.Dispose();
                        documentViewer = null;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error disposing document viewer: {ex.Message}");
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
                var settingsFolder = Path.GetDirectoryName(settingsFilePath);

                if (!Directory.Exists(settingsFolder))
                    Directory.CreateDirectory(settingsFolder!);

                if (File.Exists(settingsFilePath))
                {
                    var lastPath = File.ReadAllText(settingsFilePath);
                    if (lastPath.HasValue() && Directory.Exists(lastPath))
                        ObservableState.SetCurrentFolder(lastPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading last directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves the current directory path to settings
        /// </summary>
        void SaveLastDirectory(string path)
        {
            try
            {
                var settingsFolder = Path.GetDirectoryName(settingsFilePath);
                if (!Directory.Exists(settingsFolder))
                    Directory.CreateDirectory(settingsFolder!);

                File.WriteAllText(settingsFilePath, path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving last directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles folder browsing
        /// </summary>
        void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            ObservableState.Current.CurrentFolder.Match(
                Some: folder => folderBrowserDialog.InitialDirectory = folder.Value,
                None: () => { });

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                ObservableState.SetCurrentFolder(folderBrowserDialog.SelectedPath);
                SaveLastDirectory(folderBrowserDialog.SelectedPath);
            }
        }

        /// <summary>
        /// Updates the status label with text, ensuring UI thread safety
        /// </summary>
        void UpdateStatus(string text)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(() => UpdateStatus(text));
                return;
            }

            lblStatus.Text = text.Length > 100 ? text.Substring(0, 97) + "..." : text;
        }

        /// <summary>
        /// Handles the SortByYear checkbox change
        /// </summary>
        void ChkSortByYear_Changed(object sender, EventArgs e) =>
            ObservableState.SetSortByYear(chkSortByYear.Checked);

        /// <summary>
        /// Handles the KeepParentFolder checkbox change
        /// </summary>
        void chkKeepParentFolder_Changed(object sender, EventArgs e) =>
            ObservableState.SetKeepParentFolder(chkKeepParentFolder.Checked);

        /// <summary>
        /// Handles file scanning
        /// </summary>
        async void btnScanFiles_Click(object sender, EventArgs e)
        {
            if (ObservableState.Current.Files.Keys.Any())
            {
                var confirmResult = MessageBox.Show(this, "You currenlt have files in session, this will reset and you will lose any work in progress",
              "Confirm Rescan", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirmResult != DialogResult.Yes)
                    return;
            }
            StateSerialiser.DeleteState();

            await Task.Run(() =>
                ObservableState.Current.CurrentFolder.Match(
                    Some: async path => await ScanDirectoryAsync(path.Value),
                    None: () => MessageBox.Show("Please select a folder first.",
                                               "No folder selected",
                                               MessageBoxButtons.OK,
                                               MessageBoxIcon.Warning)));

            void OnError(Error error)
            {
                UpdateStatus("Status: Error during scan.");
                ShowMessageBox(
                    $"An error occurred while scanning: {error.Message}",
                    "Scan Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Debug.WriteLine($"Error details: {error}");
            }

            /// <summary>
            /// Scans a directory for media files
            /// </summary>
            async Task ScanDirectoryAsync(string path)
            {
                try
                {
                    ObservableState.SetWorkInProgress(true);
                    UpdateStatus("Status: Scanning files...");

                    var result = await mediaService.ScanDirectoryAsync(path);

                    result.Match(
                        Right: fileResponse =>
                        {
                            var fileCount = fileResponse.Files.Length;
                            UpdateStatus($"Status: Found {fileCount} media files.");

                            if (fileResponse.UserErrors.Length > 0)
                                ErrorListForm.ShowErrors(
                                    this,
                                    "Scan Warnings",
                                    fileResponse.UserErrors);
                        },
                        Left: OnError);

                }
                catch (Exception ex)
                {
                    OnError(Error.New(ex));
                }
                finally
                {
                    ObservableState.SetWorkInProgress(false);
                }
            }
        }
        /// <summary>
        /// Handles the Organise Files button click
        /// </summary>
        async void btnOrganiseFiles_Click(object sender, EventArgs e)
        {
            var files = from f in ObservableState.Current.Files.Values
                        where f.State != FileState.Undecided
                        select f;

            var fileCount = files.Count();

            if (fileCount == 0)
            {
                ShowMessageBox(
                    "No files marked for keeping or deletion.",
                    "No Files to Organise",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var binCount = mediaService.CountFilesForDeletion();
            var keepCount = fileCount - binCount;
            var copyOnly = ObservableState.Current.CopyOnly;

            var confirmMessage = copyOnly.Value
                ? $"You are about to:{Environment.NewLine}{Environment.NewLine}" +
                  $"• Copy {keepCount} file(s) marked for keeping{Environment.NewLine}" +
                  $"• No files will be deleted (Copy only mode){Environment.NewLine}{Environment.NewLine}" +
                  "Do you want to continue?"
                : $"You are about to:{Environment.NewLine}{Environment.NewLine}" +
                  $"• Move {keepCount} file(s) marked for keeping{Environment.NewLine}" +
                  $"• Delete {binCount} file(s) marked for deletion{Environment.NewLine}{Environment.NewLine}" +
                  "Do you want to continue?";  // todo this could be neater

            var confirmResult = MessageBox.Show(this, confirmMessage,
                "Confirm Organisation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes)
                return;

            var destinationDialog = new FolderBrowserDialog
            {
                Description = "Select destination folder for organised files",
                UseDescriptionForTitle = true
            };

            if (destinationDialog.ShowDialog() != DialogResult.OK)
                return;

            void OnError(Error error)
            {
                UpdateStatus("Status: Unexpected error during file organisation.");
                ShowMessageBox(
                    $"An unexpected error occurred: {error.Message}",
                    "Unexpected Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Debug.WriteLine($"Error details: {error}");
            }

            try
            {
                ObservableState.SetWorkInProgress(true);
                UpdateStatus("Status: Organising files...");

                var result = await mediaService.OrganizeFilesAsync(destinationDialog.SelectedPath);

                result.Match(
                    Right: organiseResponse =>
                    {
                        UpdateStatus($"Media organised.");

                        if (organiseResponse.UserErrors.Length > 0)
                        {
                            ErrorListForm.ShowErrors(
                                this,
                                "Organise Warnings",
                                organiseResponse.UserErrors);
                        }
                        else
                        {
                            // Only clear state if complete success
                            ObservableState.ClearFiles();
                            StateSerialiser.DeleteState();
                            ShowMessageBox("Organise complete", "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    },
                    Left: OnError);

            }
            catch (Exception ex)
            {
                OnError(Error.New(ex));
            }
            finally
            {
                ObservableState.SetWorkInProgress(false);                
            }
        }

        /// <summary>
        /// Handles the CopyOnly checkbox change
        /// </summary>
        void chkCopyOnly_Changed(object sender, EventArgs e) =>
            ObservableState.SetCopyOnly(chkCopyOnly.Checked);

        /// <summary>
        /// Handles the Previous button click
        /// </summary>
        void btnPrevious_Click(object sender, EventArgs e) =>
            ObservableState.PreviousFile();

        /// <summary>
        /// Handles the Next button click
        /// </summary>
        void btnNext_Click(object sender, EventArgs e) =>
            ObservableState.NextFile();

        /// <summary>
        /// Handles the Bin button click
        /// </summary>
        void btnBin_Click(object sender, EventArgs e) =>
            ObservableState.UpdateFileState(FileState.Bin);

        /// <summary>
        /// Handles the Keep button click
        /// </summary>
        void btnKeep_Click(object sender, EventArgs e) =>
            ObservableState.UpdateFileState(FileState.Keep);

        /// <summary>
        /// Shows a message box with UI thread safety
        /// </summary>
        void ShowMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            if (InvokeRequired)
                Invoke(() => MessageBox.Show(this, message, caption, buttons, icon));
            else
                MessageBox.Show(this, message, caption, buttons, icon);
        }

        private void tbFileName_TextChanged(object sender, EventArgs e) =>
            ObservableState.UpdateFilename(tbFileName.Text);
    }
}