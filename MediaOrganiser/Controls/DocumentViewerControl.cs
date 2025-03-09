using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using IronPdf;
using System.Collections.Generic;
using Image = System.Drawing.Image;

namespace MediaOrganiser
{
    /// <summary>
    /// A document viewer control with integrated PDF display using IronPDF
    /// and Word document rendering via conversion to PDF
    /// </summary>
    public class DocumentViewerControl : UserControl
    {
        // UI components
        private TextBox txtDocumentContent;
        private Panel loadingPanel;
        private Label loadingLabel;
        private Panel pdfPanel;
        private PictureBox pdfPictureBox;
        private Panel pdfNavigationPanel;
        private Button btnPrevPage;
        private Button btnNextPage;
        private Label lblPageInfo;

        // PDF rendering components
        private PdfDocument currentPdfDocument;
        private Image[] pdfPageImages;
        private int currentPdfPage = 0;
        private int totalPdfPages = 0;

        // Current document path
        private string currentDocumentPath;
        private string currentDocumentExt;

        // Temp file for Word to PDF conversion
        private string tempPdfPath;

        // Synchronization context for UI thread operations
        private SynchronizationContext syncContext;

        /// <summary>
        /// Creates and initializes the document viewer control
        /// </summary>
        public DocumentViewerControl()
        {
            // Capture the synchronization context of the UI thread
            syncContext = SynchronizationContext.Current ?? new SynchronizationContext();

            InitializeComponents();
        }

        /// <summary>
        /// Initializes all UI components for document viewing
        /// </summary>
        void InitializeComponents()
        {
            // Create text viewer for plain text documents
            txtDocumentContent = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.White,
                Visible = false
            };

            // Create loading indicator
            loadingPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackColor = System.Drawing.Color.White
            };

            loadingLabel = new Label
            {
                Text = "Loading document...",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font(Font.FontFamily, 12)
            };

            loadingPanel.Controls.Add(loadingLabel);

            // Create PDF navigation panel
            pdfNavigationPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = System.Drawing.Color.LightGray,
                Visible = false
            };

            btnPrevPage = new Button
            {
                Text = "Previous",
                Width = 80,
                Height = 30,
                Location = new System.Drawing.Point(10, 5),
                Enabled = false
            };

            btnNextPage = new Button
            {
                Text = "Next",
                Width = 80,
                Height = 30,
                Location = new System.Drawing.Point(100, 5),
                Enabled = false
            };

            lblPageInfo = new Label
            {
                Text = "Page: 0 / 0",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new System.Drawing.Point(200, 10)
            };

            btnPrevPage.Click += (s, e) => ShowPdfPage(currentPdfPage - 1);
            btnNextPage.Click += (s, e) => ShowPdfPage(currentPdfPage + 1);

            pdfNavigationPanel.Controls.Add(btnPrevPage);
            pdfNavigationPanel.Controls.Add(btnNextPage);
            pdfNavigationPanel.Controls.Add(lblPageInfo);

            // Create panel for PDF display
            pdfPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackColor = System.Drawing.Color.White
            };

            pdfPictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = System.Drawing.Color.White,
                Visible = true
            };

            pdfPanel.Controls.Add(pdfPictureBox);

            // Initialize the IronPdf license if you have one
            try
            {
                // If you have a license, uncomment this line and add your license key
                // IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

                // Configure IronPDF rendering
                Installation.TempFolderPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MediaOrganiser", "IronPdfTemp");

                // Create the directory if it doesn't exist
                if (!Directory.Exists(Installation.TempFolderPath))
                {
                    Directory.CreateDirectory(Installation.TempFolderPath);
                }

                Debug.WriteLine("IronPDF initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing IronPDF: {ex.Message}");
            }

            // Add controls to this control
            Controls.Add(txtDocumentContent);
            Controls.Add(loadingPanel);
            Controls.Add(pdfPanel);
            Controls.Add(pdfNavigationPanel);
        }

        /// <summary>
        /// Safely executes an action on the UI thread
        /// </summary>
        private void SafeInvokeOnUIThread(Action action)
        {
            if (IsDisposed)
            {
                Debug.WriteLine("Control is disposed, not executing UI action");
                return;
            }

            try
            {
                if (InvokeRequired)
                {
                    syncContext.Post(_ => {
                        if (!IsDisposed)
                            action();
                    }, null);
                }
                else
                {
                    action();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error invoking action on UI thread: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads and displays a document based on its file extension
        /// </summary>
        public void LoadDocument(string filePath)
        {
            try
            {
                // Store the document path
                currentDocumentPath = filePath;
                currentDocumentExt = Path.GetExtension(filePath).ToLower();

                // Reset state
                ResetViewers();

                // Make sure file exists
                if (!File.Exists(filePath))
                {
                    ShowErrorMessage($"File not found: {filePath}");
                    return;
                }

                // Show loading indicator while we process the document
                loadingPanel.Visible = true;

                // Process by file type
                switch (currentDocumentExt)
                {
                    case ".txt":
                        LoadTextDocument(filePath);
                        break;
                    case ".pdf":
                        LoadPdfDocumentAsync(filePath);
                        break;
                    case ".doc":
                    case ".docx":
                        LoadWordDocumentAsync(filePath);
                        break;
                    default:
                        LoadUnsupportedDocument(filePath);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading document: {ex.Message}");
                ShowErrorMessage($"Cannot display this document: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads and displays a plain text document
        /// </summary>
        void LoadTextDocument(string filePath)
        {
            try
            {
                // Read text file with appropriate encoding detection
                var text = File.ReadAllText(filePath, Encoding.UTF8);

                SafeInvokeOnUIThread(() => {
                    txtDocumentContent.Text = text;
                    txtDocumentContent.Visible = true;
                    loadingPanel.Visible = false;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading text document: {ex.Message}");
                ShowErrorMessage($"Error loading text file: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads a PDF document using IronPDF
        /// </summary>
        async void LoadPdfDocumentAsync(string filePath)
        {
            try
            {
                // Load PDF on a background thread to keep UI responsive
                await Task.Run(() => {
                    try
                    {
                        // Load the PDF using IronPDF
                        currentPdfDocument = new PdfDocument(filePath);
                        totalPdfPages = currentPdfDocument.PageCount;

                        var images = currentPdfDocument.ToBitmap(150);
                        var pages = new List<System.Drawing.Image>();

                        foreach (var img in images)
                        {
                            pages.Add(img);
                        }

                        pdfPageImages = pages.ToArray();

                        // Show the first page
                        currentPdfPage = 0;

                        SafeInvokeOnUIThread(() => {
                            ShowPdfPage(0);
                            pdfPanel.Visible = true;
                            pdfNavigationPanel.Visible = true;
                            loadingPanel.Visible = false;
                            Debug.WriteLine("PDF loaded successfully with IronPDF");
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error rendering PDF with IronPDF: {ex.Message}");
                        SafeInvokeOnUIThread(() => {
                            ShowErrorMessage($"Error rendering PDF: {ex.Message}");
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PDF loading error: {ex.Message}");
                ShowErrorMessage($"Error loading PDF: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads and displays a Word document by converting it to PDF using IronPDF
        /// </summary>
        async void LoadWordDocumentAsync(string filePath)
        {
            try
            {
                SafeInvokeOnUIThread(() => {
                    loadingLabel.Text = "Converting Word document...";
                });

                // Convert and load on a background thread to keep UI responsive
                await Task.Run(() => {
                    try
                    {
                       
                        // Convert Word to PDF using IronPDF's built-in converter
                        var renderer = new DocxToPdfRenderer();
                        // Render from DOCX file
                        var pdf = renderer.RenderDocxAsPdf(filePath);
                        var pdfDocument = PdfDocument.FromFile(filePath);
                                        
                        totalPdfPages = pdfDocument.PageCount;

                        var images = pdfDocument.ToBitmap(150);
                        var pages = new List<System.Drawing.Image>();

                        foreach (var img in images)
                        {
                            pages.Add(img);
                        }

                        pdfPageImages = pages.ToArray();

                        // Show the first page
                        currentPdfPage = 0;

                        SafeInvokeOnUIThread(() => {
                            ShowPdfPage(0);
                            pdfPanel.Visible = true;
                            pdfNavigationPanel.Visible = true;
                            loadingPanel.Visible = false;
                            Debug.WriteLine("Word document converted and loaded successfully");
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error converting Word document: {ex.Message}");
                        SafeInvokeOnUIThread(() => {
                            // Fall back to external app if conversion fails
                            CleanupPdfResources();
                            ShowWordExternalOpenPanel(filePath);
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Word document loading error: {ex.Message}");
                ShowErrorMessage($"Error loading Word document: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a panel with button to open Word document externally
        /// </summary>
        void ShowWordExternalOpenPanel(string filePath)
        {
            SafeInvokeOnUIThread(() => {
                // Create a simple panel with a button to open Word doc
                Panel wordPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = System.Drawing.Color.White
                };

                Label infoLabel = new Label
                {
                    Text = $"Word Document: {Path.GetFileName(filePath)}",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 40,
                    Font = new Font(Font.FontFamily, 12, FontStyle.Bold)
                };

                Label errorLabel = new Label
                {
                    Text = "Unable to render document in viewer. You can open it externally:",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 40,
                    Font = new Font(Font.FontFamily, 10)
                };

                Button openButton = new Button
                {
                    Text = "Open in Microsoft Word",
                    Width = 200,
                    Height = 40,
                    Anchor = AnchorStyles.None
                };

                openButton.Click += (s, e) => OpenDocumentExternally(filePath);

                // Center the button
                openButton.Location = new System.Drawing.Point(
                    (wordPanel.Width - openButton.Width) / 2,
                    (wordPanel.Height - openButton.Height) / 2);

                // Handle resize
                wordPanel.Resize += (s, e) =>
                {
                    openButton.Location = new System.Drawing.Point(
                        (wordPanel.Width - openButton.Width) / 2,
                        (wordPanel.Height - openButton.Height) / 2);
                };

                // Add controls in reverse order for stacking
                wordPanel.Controls.Add(openButton);
                wordPanel.Controls.Add(errorLabel);
                wordPanel.Controls.Add(infoLabel);

                Controls.Add(wordPanel);
                wordPanel.BringToFront();
                loadingPanel.Visible = false;
            });
        }

        /// <summary>
        /// Displays a specific page of the PDF
        /// </summary>
        private void ShowPdfPage(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= pdfPageImages.Length)
            {
                pageIndex = 0;
            }
   
            try
            {
                // Clean up old image if there is one
                if (pdfPictureBox.Image != null && pdfPictureBox.Image != pdfPageImages[pageIndex])
                {
                    // Don't dispose the image as we're keeping them in the pdfPageImages list
                    pdfPictureBox.Image = null;
                }

                // Show the new page
                pdfPictureBox.Image = pdfPageImages[pageIndex];
                currentPdfPage = pageIndex;
                lblPageInfo.Text = $"Page: {pageIndex + 1} / {totalPdfPages}";

                // Update button states
                btnPrevPage.Enabled = pageIndex > 0;
                btnNextPage.Enabled = pageIndex < totalPdfPages - 1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing PDF page: {ex.Message}");
            }
        }

        /// <summary>
        /// Opens a document in its associated external application
        /// </summary>
        private void OpenDocumentExternally(string filePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening file externally: {ex.Message}");
                MessageBox.Show($"Could not open the document: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Shows a message for unsupported document types
        /// </summary>
        void LoadUnsupportedDocument(string filePath)
        {
            ShowErrorMessage($"Preview not available for {Path.GetExtension(filePath).ToUpper()} files.");

            SafeInvokeOnUIThread(() => {
                loadingPanel.Visible = false;
            });
        }

        /// <summary>
        /// Displays an error message in the text viewer
        /// </summary>
        void ShowErrorMessage(string message)
        {
            Debug.WriteLine($"Document viewer error: {message}");

            SafeInvokeOnUIThread(() => {
                pdfPanel.Visible = false;
                pdfNavigationPanel.Visible = false;
                loadingPanel.Visible = false;
                txtDocumentContent.Text = message;
                txtDocumentContent.Visible = true;
            });
        }

        /// <summary>
        /// Resets all viewers to hidden state
        /// </summary>
        void ResetViewers()
        {
            SafeInvokeOnUIThread(() => {
                // Hide existing viewers
                txtDocumentContent.Visible = false;
                txtDocumentContent.Text = string.Empty;
                pdfPanel.Visible = false;
                pdfNavigationPanel.Visible = false;

                // Clear the PDF PictureBox without disposing the image
                if (pdfPictureBox.Image != null)
                {
                    pdfPictureBox.Image = null;
                }

                loadingPanel.Visible = false;

                // Remove any additional panels that might have been added
                for (int i = Controls.Count - 1; i >= 0; i--)
                {
                    Control control = Controls[i];
                    if (control != txtDocumentContent &&
                        control != pdfPanel &&
                        control != loadingPanel &&
                        control != pdfNavigationPanel)
                    {
                        Controls.RemoveAt(i);
                        control.Dispose();
                    }
                }
            });
        }

        /// <summary>
        /// Clean up PDF resources
        /// </summary>
        private void CleanupPdfResources()
        {
            try
            {
                // Clean up PDF page images
                if (pdfPageImages != null)
                {
                    foreach (var image in pdfPageImages)
                    {
                        image?.Dispose();
                    }
                    pdfPageImages = new Image[0];
                }

                // Dispose the PDF document
                currentPdfDocument?.Dispose();
                currentPdfDocument = null;

                // Delete temporary PDF if it exists
                if (!string.IsNullOrEmpty(tempPdfPath) && File.Exists(tempPdfPath))
                {
                    try
                    {
                        File.Delete(tempPdfPath);
                        tempPdfPath = null;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error deleting temporary PDF: {ex.Message}");
                    }
                }

                // Reset page counters
                currentPdfPage = 0;
                totalPdfPages = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cleaning up PDF resources: {ex.Message}");
            }
        }

        /// <summary>
        /// Clean up resources
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    Debug.WriteLine("Disposing DocumentViewerControl");

                    // Close any open documents
                    ResetViewers();

                    // Clean up PDF resources
                    CleanupPdfResources();

                    // Dispose of controls
                    txtDocumentContent?.Dispose();
                    loadingPanel?.Dispose();
                    loadingLabel?.Dispose();
                    pdfPanel?.Dispose();
                    pdfPictureBox?.Dispose();
                    pdfNavigationPanel?.Dispose();
                    btnPrevPage?.Dispose();
                    btnNextPage?.Dispose();
                    lblPageInfo?.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error disposing document viewer: {ex.Message}");
                }
            }

            base.Dispose(disposing);
        }
    }
}