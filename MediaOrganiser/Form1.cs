using LanguageExt;
using LanguageExt.Common;
using MaterialSkin;
using MaterialSkin.Controls;
using MediaOrganiser.Services;
using MusicTools.Logic;
using SortPhotos.Core;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using static SortPhotos.Core.Types;
using static LanguageExt.Prelude;

namespace MediaOrganiser
{
    public partial class Form1 : MaterialForm
    {
        readonly MediaService mediaService;
        System.Drawing.Image? currentImage;

        readonly MaterialSkinManager materialSkinManager;

        VideoPlayerControl? videoPlayer;
        DocumentViewerControl? documentViewer;
        Panel? openFolderPanel;
        MaterialButton? openFolderButton;

        readonly string settingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MediaOrganiser",
            "settings.json");

        public Form1()
        {
            InitializeComponent();

            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;

            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.Blue600, Primary.Blue700,
                Primary.Blue500, Accent.LightBlue200,
                TextShade.WHITE);

            mediaService = new MediaService();

            ObservableState.StateChanged += OnStateChanged;
            InitializeVideoPlayer();
            InitializeDocumentViewer();
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
            btnKeep.Enabled = !state.WorkInProgress && state.Files.Count > 0;

            var currentFileIndex = state.CurrentFile.Map(i => i.Value).IfNone(0);
            btnPrevious.Enabled = !state.WorkInProgress && state.Files.Count > 0 && currentFileIndex > 1;

            btnNext.Enabled = !state.WorkInProgress && state.Files.Count > 0 &&
                              currentFileIndex > 0 && currentFileIndex < state.Files.Count;

            btnOrganiseFiles.Enabled = !state.WorkInProgress &&
                (from f in state.Files.Values
                 where f.State == FileState.Keep || 
                       f.State == FileState.Bin
                 select unit).Any();

            progressScan.Style = state.WorkInProgress ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
            progressScan.MarqueeAnimationSpeed = state.WorkInProgress ? 30 : 0;
            if (!state.WorkInProgress) progressScan.Value = 0;

            switchCopyOnly.Checked = state.CopyOnly;

            UpdateMediaDisplay(state);

            if (!state.WorkInProgress)
                StateSerializer.SaveState(state);
        }

        /// <summary>
        /// Updates the displayed media based on the current file selection
        /// </summary>
        void UpdateMediaDisplay(AppModel state)
        {
            StopMedia();

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
                    var fullFileName = $"{mediaInfo.FileName.Value}.{mediaInfo.Extension.Value.TrimStart('.')}";

                    if (mediaInfo.Category == FileCategory.Image)
                    {
                        DisplayImage(mediaInfo);
                        UpdateStatus($"Current file: {fullFileName} ({fileSizeText}) - {fileStatus}");
                    }
                    else if (mediaInfo.Category == FileCategory.Video)
                    {
                        DisplayVideo(mediaInfo);
                        UpdateStatus($"Current file: {fullFileName} (Video, {fileSizeText}) - {fileStatus}");
                    }
                    else if (mediaInfo.Category == FileCategory.Document)
                    {
                        DisplayDocument(mediaInfo);
                        UpdateStatus($"Current file: {fullFileName} (Document, {fileSizeText}) - {fileStatus}");
                    }
                },
                None: () => UpdateStatus("No files selected"));
        }

        /// <summary>
        /// Displays a document file
        /// </summary>
        void DisplayDocument(MediaInfo mediaInfo)
        {
            try
            {
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

        /// <summary>
        /// Shows error message with button to open file's directory
        /// </summary>
        void ShowErrorWithOpenFolderButton(MediaInfo mediaInfo)
        {
            if (openFolderPanel == null)
            {
                openFolderPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = System.Drawing.Color.WhiteSmoke
                };

                var errorLabel = new MaterialLabel
                {
                    Text = "Unable to preview file",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 40,
                    Font = new Font(Font.FontFamily, 12, FontStyle.Bold)
                };

                openFolderButton = new MaterialButton
                {
                    Text = "Open enclosing folder",
                    Dock = DockStyle.Top,
                    Height = 40,
                    Width = 200,
                    Type = MaterialButton.MaterialButtonType.Contained
                };

                openFolderButton.Click += (s, e) => {
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
                if (videoPlayer != null) videoPlayer.Visible = false;
                if (documentViewer != null) documentViewer.Visible = false;
                if (openFolderPanel != null) openFolderPanel.Visible = false;
                picCurrentImage.Visible = true;

                if (currentImage != null)
                {
                    currentImage.Dispose();
                    currentImage = null;
                }

                currentImage = System.Drawing.Image.FromFile(mediaInfo.FullPath.Value);
                picCurrentImage.Image = currentImage;
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

        /// <summary>
        /// Loads previous session or directory on form load
        /// </summary>
        void Form1_Load(object sender, EventArgs e)
        {
            var savedState = StateSerializer.LoadState();

            savedState.IfSome(state =>
            {
                var result = MaterialMessageBox.Show(
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
                    StateSerializer.DeleteState();
                }
            });

            LoadLastDirectory();
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
        /// Handles file scanning
        /// </summary>
        async void btnScanFiles_Click(object sender, EventArgs e)
        {
            StateSerializer.DeleteState();

            await Task.Run(() =>
                ObservableState.Current.CurrentFolder.Match(
                    Some: async path => await ScanDirectoryAsync(path.Value),
                    None: () => MaterialMessageBox.Show(
                        "Please select a folder first.",
                        "No folder selected",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning)));
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
                    Right: fileResponse => {
                        var fileCount = fileResponse.Files.Length;
                        UpdateStatus($"Status: Found {fileCount} media files.");

                        if (fileResponse.UserErrors.Length > 0)
                        {
                            var errorMessages = string.Join(Environment.NewLine,
                                from ue in fileResponse.UserErrors
                                select ue.message);

                            ShowMessageBox(
                                $"Scan completed with some warnings:{Environment.NewLine}{errorMessages}",
                                "Scan Warnings",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                    },
                    Left: error => {
                        UpdateStatus("Status: Error during scan.");
                        ShowMessageBox(
                            $"An error occurred while scanning: {error.Message}",
                            "Scan Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        Debug.WriteLine($"Error details: {error}");
                    });
            }
            catch (Exception ex)
            {
                UpdateStatus("Status: Error during scan.");
                ShowMessageBox(
                    $"An unexpected error occurred: {ex.Message}",
                    "Unexpected Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Debug.WriteLine($"Exception details: {ex}");
            }
            finally
            {
                ObservableState.SetWorkInProgress(false);
            }
        }

        /// <summary>
        /// Handles the Organise Files button click
        /// </summary>
        async void btnOrganiseFiles_Click(object sender, EventArgs e)
        {
            var files = from f in ObservableState.Current.Files.Values
                        where f.State != FileState.Unprocessed
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

            var confirmMessage = copyOnly
                ? $"You are about to:{Environment.NewLine}{Environment.NewLine}" +
                  $"• Copy {keepCount} file(s) marked for keeping{Environment.NewLine}" +
                  $"• No files will be deleted (Copy only mode){Environment.NewLine}{Environment.NewLine}" +
                  "Do you want to continue?"
                : $"You are about to:{Environment.NewLine}{Environment.NewLine}" +
                  $"• Move {keepCount} file(s) marked for keeping{Environment.NewLine}" +
                  $"• Delete {binCount} file(s) marked for deletion{Environment.NewLine}{Environment.NewLine}" +
                  "Do you want to continue?";

            var confirmResult = MaterialMessageBox.Show(
                this,
                confirmMessage,
                "Confirm Organisation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes)
                return;

            var destinationDialog = new FolderBrowserDialog
            {
                Description = "Select destination folder for organised files",
                UseDescriptionForTitle = true
            };

            if (destinationDialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                ObservableState.SetWorkInProgress(true);
                UpdateStatus("Status: Organising files...");

                var result = await mediaService.OrganizeFilesAsync(destinationDialog.SelectedPath);

                result.Match(
                    Right: fileResponse => {
                        UpdateStatus($"Media organised.");

                        if (fileResponse.UserErrors.Length > 0)
                        {
                            var errorMessages = string.Join(
                                Environment.NewLine,
                                from ue in fileResponse.UserErrors
                                select ue.message);

                            ShowMessageBox(
                                $"Organisation completed with some warnings:{Environment.NewLine}{errorMessages}",
                                "Organise Warnings",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                    },
                    Left: error => {
                        UpdateStatus("Status: Error during organise.");
                        ShowMessageBox(
                            $"An error occurred while organising: {error.Message}",
                            "Organise Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        Debug.WriteLine($"Error details: {error}");
                    });
            }
            catch (Exception ex)
            {
                UpdateStatus("Status: Unexpected error during file organisation.");
                ShowMessageBox(
                    $"An unexpected error occurred: {ex.Message}",
                    "Unexpected Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Debug.WriteLine($"Exception details: {ex}");
            }
            finally
            {
                ObservableState.SetWorkInProgress(false);
            }
        }

        /// <summary>
        /// Handles the CopyOnly checkbox change
        /// </summary>
        void switchCopyOnly_CheckedChanged(object sender, EventArgs e) =>
            ObservableState.SetCopyOnly(switchCopyOnly.Checked);

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
        /// Shows a message box with UI thread safety
        /// </summary>
        void ShowMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            if (InvokeRequired)
                Invoke(() => MaterialMessageBox.Show(this, message, caption, buttons, icon));
            else
                MaterialMessageBox.Show(this, message, caption, buttons, icon);
        }
    }
}