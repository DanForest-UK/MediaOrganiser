using LanguageExt;
using LanguageExt.Common;
using MediaOrganiser.Services;
using System.Diagnostics;
using System.IO;
using static LanguageExt.Prelude;
using D = System.Drawing;
using MediaOrganiser.Logic;
using MediaOrganiser.Domain;
using MediaOrganiser.WindowsSpecific;

namespace MediaOrganiser
{
    public partial class MainForm : Form
    {
        readonly MediaService mediaService;
        Option<VideoPlayerControl> videoPlayer;
        Option<DocumentViewerControl> documentViewer;

        public MainForm()
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

        readonly string settingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MediaOrganiser",
            "settings.json");

        /// <summary>
        /// Initializes the video player control
        /// </summary>
        void InitializeVideoPlayer() =>
            Try.lift(() =>
            {
                var vp = new VideoPlayerControl();
                vp.Dock = DockStyle.Fill;
                vp.Visible = false;
                picCurrentImage.Controls.Add(vp);
                videoPlayer = vp;
            }).IfFail(ex =>
                UpdateStatus("Error: Video player could not be initialized", ex.Exception));

        /// <summary>
        /// Initializes the document viewer control
        /// </summary>
        void InitializeDocumentViewer() =>
            Try.lift(() =>
            {
                var dv = new DocumentViewerControl();
                dv.Dock = DockStyle.Fill;
                dv.Visible = false;
                picCurrentImage.Controls.Add(dv);
                documentViewer = dv;
            }).IfFail(ex =>
                UpdateStatus("Error: Document viewer could not be initialized", ex.Exception));

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

            // todo is this used?
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
            DisposeMedia();

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

                    UpdateStatus($"{placeText}{fullFileName} ({fileSizeText}) - {fileStatus}{lastFileText}", None);

                    if (mediaInfo.Category == FileCategory.Image)
                        DisplayImage(mediaInfo);

                    else if (mediaInfo.Category == FileCategory.Video)
                        DisplayVideo(mediaInfo);

                    else if (mediaInfo.Category == FileCategory.Document)
                        DisplayDocument(mediaInfo);

                },
                None: () =>
                {
                    UpdateStatus("No files selected", None);
                    // Hide the center controls panel when no file is selected
                    pnlCenterControls.Visible = false;
                });
        }

        /// <summary>
        /// Displays a document file
        /// </summary>
        void DisplayDocument(MediaInfo mediaInfo) =>
            Try.lift(() =>
            {
                InitializeDocumentViewer();
                documentViewer.IfSome(dv =>
                {
                    dv.Visible = true;
                    dv.BringToFront();
                    dv.LoadDocument(mediaInfo.FullPath.Value);
                });
                return unit;
            }).IfFail(ex =>
                UpdateStatus("Error displaying document", ex.Exception));

        void btnRotateLeft_Click(object sender, EventArgs e) =>
            ObservableState.RotateCurrentImage(Rotation.Rotate270); // Counterclockwise

        void btnRotateRight_Click(object sender, EventArgs e) =>
            ObservableState.RotateCurrentImage(Rotation.Rotate90); // Clockwise

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
        void DisplayImage(MediaInfo mediaInfo) =>
            Try.lift(() =>
            {
                btnRotateLeft.Visible = true;
                btnRotateRight.Visible = true;
                picCurrentImage.Visible = true;

                var rotationResult = Try.lift(() => Images.RotateImage(mediaInfo.FullPath.Value, mediaInfo.Rotation).Run());

                rotationResult.Match(
                    Succ: rotatedImage =>
                    {
                        picCurrentImage.Image = rotatedImage;
                        return unit;
                    },
                    Fail: ex =>
                    {
                        picCurrentImage.Image = D.Image.FromFile(mediaInfo.FullPath.Value);
                        return UpdateStatus("Unable to rotate image", ex.Exception);
                    });

                // Show/hide rotation buttons for images only
                btnRotateLeft.Visible = true;
                btnRotateRight.Visible = true;
            }).IfFail(ex =>
                UpdateStatus($"Error displaying image: {mediaInfo.FullPath.Value}", ex.Exception));

        /// <summary>
        /// Displays and plays a video file
        /// </summary>
        void DisplayVideo(MediaInfo mediaInfo) =>
            Try.lift(() =>
            {
                InitializeVideoPlayer();

                videoPlayer.IfSome(vp =>
                {
                    vp.Visible = true;
                    vp.SetSource(mediaInfo.FullPath.Value);
                    vp.Play();
                });
            }).IfFail(ex =>
                UpdateStatus($"Error playing video: {mediaInfo.FileName.Value}", ex.Exception));

        void HideImageUI()
        {
            btnRotateLeft.Visible = false;
            btnRotateRight.Visible = false;
            picCurrentImage.Image = null;
        }

        /// <summary>
        /// Stops and disposes all media viewers
        /// </summary>
        void DisposeMedia()
        {
            DisposeVideoPlayer();
            DisposeDocumentViewer();
            HideImageUI();
        }

        /// <summary>
        /// Checks to see if saved state and loads if requested
        /// </summary>
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
                                UpdateStatus("Previous session loaded", None);
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

                DisposeMedia();

                if (disposing && (components != null))
                    components.Dispose();

                base.Dispose(disposing);
            }
        }

        void DisposeVideoPlayer()
        {
            videoPlayer.IfSome(vp =>
                Try.lift(() =>
                {
                    vp.Stop();
                    vp.Dispose();
                }).IfFail(ex =>
                {
                    Debug.WriteLine($"Error disposing video player: {ex.Message}");
                    return unit;
                }));
            videoPlayer = None;
        }


        void DisposeDocumentViewer()
        {
            documentViewer.IfSome(dv =>
                Try.lift(() =>
                {
                    dv.Dispose();
                }).IfFail(ex =>
                {
                    Debug.WriteLine($"Error disposing document viewer: {ex.Message}");
                    return unit;
                }));

           documentViewer = None;
        }


        /// <summary>
        /// Loads the last used directory path from settings
        /// </summary>
        void LoadLastDirectory() =>
            Try.lift(() =>
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
            }).IfFail(ex =>
            {
                Debug.WriteLine($"Error loading last directory: {ex.Message}");
                return unit;
            });

        /// <summary>
        /// Saves the current directory path to settings
        /// </summary>
        void SaveLastDirectory(string path) =>
            Try.lift(() =>
            {
                var settingsFolder = Path.GetDirectoryName(settingsFilePath);
                if (!Directory.Exists(settingsFolder))
                    Directory.CreateDirectory(settingsFolder!);

                File.WriteAllText(settingsFilePath, path);
            }).IfFail(ex =>
            {
                Debug.WriteLine($"Error saving last directory: {ex.Message}");
                return unit;
            });

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
        Unit UpdateStatus(string text, Option<Exception> exception)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(() => UpdateStatus(text, exception));
                return unit;
            }

            lblStatus.Text = text.Length > 100 ? text.Substring(0, 97) + "..." : text;

            exception.IfSome(ex => Debug.Write(ex));
            return unit;
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
                UpdateStatus("Status: Error during scan.", None);
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
            async Task ScanDirectoryAsync(string path) =>
                await Try.lift(async () =>
                {
                    ObservableState.SetWorkInProgress(true);
                    UpdateStatus("Status: Scanning files...", None);

                    var result = await mediaService.ScanDirectoryAsync(path);

                    result.Match(
                        Right: fileResponse =>
                        {
                            var fileCount = fileResponse.Files.Length;
                            UpdateStatus($"Status: Found {fileCount} media files.", None);

                            if (fileResponse.UserErrors.Length > 0)
                                ErrorListForm.ShowErrors(
                                    this,
                                    "Scan Warnings",
                                    fileResponse.UserErrors);
                        },
                        Left: OnError);

                }).IfFail(ex =>
                {
                    OnError(Error.New(ex));
                    return Task.FromResult(unit);
                });

            ObservableState.SetWorkInProgress(false);
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

            var binCount = ObservableState.Current.CountFilesForDeletion();
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
                UpdateStatus("Status: Unexpected error during file organisation.", None);
                ShowMessageBox(
                    $"An unexpected error occurred: {error.Message}",
                    "Unexpected Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Debug.WriteLine($"Error details: {error}");
            }

            await Try.lift(async () =>
            {
                ObservableState.SetWorkInProgress(true);
                UpdateStatus("Status: Organising files...", None);

                var result = await mediaService.OrganizeFilesAsync(destinationDialog.SelectedPath);

                result.Match(
                    Right: organiseResponse =>
                    {
                        UpdateStatus($"Media organised.", None);

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
                return unit;
            }).IfFail(ex =>
            {
                OnError(Error.New(ex));
                return Task.FromResult(unit);
            });                
              
            ObservableState.SetWorkInProgress(false);    
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