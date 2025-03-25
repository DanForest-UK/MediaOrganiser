using LanguageExt;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using IronPdf;
using System.Collections.Generic;
using System.Drawing;
using Image = System.Drawing.Image;
using static MediaOrganiser.Core.Types;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using static LanguageExt.Prelude;

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
        private RichTextBox rtfDocumentContent;
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

        // Cancellation token source for document loading operations
        private CancellationTokenSource loadingCts;

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
                Visible = false
            };
            ThemeManager.StyleTextBox(txtDocumentContent);

            // Create rich text viewer for RTF documents
            rtfDocumentContent = new RichTextBox
            {
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Visible = false
            };
            ThemeManager.StyleRichTextBox(rtfDocumentContent);

            // Create loading indicator
            loadingPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false
            };
            ThemeManager.StylePanel(loadingPanel, ThemeManager.PrimaryBackColor);

            loadingLabel = new Label
            {
                Text = "Loading document...",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            ThemeManager.StyleLabel(loadingLabel, ThemeManager.HeaderFont);

            loadingPanel.Controls.Add(loadingLabel);

            // Create PDF navigation panel
            // Create navigation panel to match exactly the main form's panel
            pdfNavigationPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58, // Match exactly with Form1's pnlButtons height
                Visible = false,
                BackColor = ThemeManager.SecondaryBackColor,
                Padding = new Padding(6, 6, 6, 6) // Match exactly with Form1's pnlButtons padding
            };

            // Create buttons that exactly match the main form's buttons
            btnPrevPage = new Button
            {
                Text = "Back",
                Size = new Size(130, 45), // Match size with Form1
                Enabled = false,
                Location = new Point(4, 4),
                Margin = new Padding(4, 4, 4, 4), // Explicit margin to match Form1
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            btnNextPage = new Button
            {
                Text = "Next",
                Size = new Size(130, 45), // Match size with Form1
                Enabled = false,
                Location = new Point(150, 4),
                Margin = new Padding(4, 4, 4, 4), // Explicit margin to match Form1
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            // Apply theme styling
            ThemeManager.StyleSecondaryButton(btnPrevPage);
            ThemeManager.StyleSecondaryButton(btnNextPage);

            lblPageInfo = new Label
            {
                Text = "Page: 0 / 0",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(290, 13), // Adjusted to be after the buttons
                ForeColor = Color.Black,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            ThemeManager.StyleLabel(lblPageInfo);

            btnPrevPage.Click += (s, e) => ShowPdfPage(currentPdfPage - 1);
            btnNextPage.Click += (s, e) => ShowPdfPage(currentPdfPage + 1);

            // Add controls directly to the panel
            pdfNavigationPanel.Controls.Add(lblPageInfo);
            pdfNavigationPanel.Controls.Add(btnNextPage);
            pdfNavigationPanel.Controls.Add(btnPrevPage);

            // Create panel for PDF display
            pdfPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false
            };
            ThemeManager.StylePanel(pdfPanel, ThemeManager.PrimaryBackColor);

            pdfPictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Visible = true
            };
            pdfPictureBox.BackColor = ThemeManager.PrimaryBackColor;

            pdfPanel.Controls.Add(pdfPictureBox);

            // Initialize the IronPdf license if you have one
            InitializeIronPdf();

            // Add controls to this control
            Controls.Add(txtDocumentContent);
            Controls.Add(rtfDocumentContent);
            Controls.Add(loadingPanel);
            Controls.Add(pdfPanel);
            Controls.Add(pdfNavigationPanel);
        }

        /// <summary>
        /// Initialize IronPdf library
        /// </summary>
        private Unit InitializeIronPdf() =>
            Try.lift(() =>
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
                return unit;
            }).IfFail(ex =>
            {
                Debug.WriteLine($"Error initializing IronPDF: {ex.Message}");
                return unit;
            });

        /// <summary>
        /// Safely executes an action on the UI thread
        /// </summary>
        private Unit SafeInvokeOnUIThread(Action action) =>
            Try.lift(() =>
            {
                if (IsDisposed)
                {
                    Debug.WriteLine("Control is disposed, not executing UI action");
                    return unit;
                }

                if (InvokeRequired)
                {
                    syncContext.Post(_ =>
                    {
                        if (!IsDisposed)
                            action();
                    }, null);
                }
                else
                {
                    action();
                }
                return unit;
            }).IfFail(ex =>
            {
                Debug.WriteLine($"Error invoking action on UI thread: {ex.Message}");
                return unit;
            });

        /// <summary>
        /// Loads and displays a document based on its file extension
        /// </summary>
        public void LoadDocument(string filePath)
        {
            Try.lift(() =>
            {
                // Cancel any existing document loading operation
                loadingCts?.Cancel();
                loadingCts = new CancellationTokenSource();

                // Store the document path
                currentDocumentPath = filePath;
                currentDocumentExt = Path.GetExtension(filePath).ToLower();

                // Make sure file exists
                if (!File.Exists(filePath))
                {
                    ShowErrorMessage($"File not found: {filePath}");
                    return unit;
                }

                // Show loading indicator while we process the document
                SafeInvokeOnUIThread(() =>
                {
                    loadingPanel.Visible = true;
                });

                // Process by file type
                switch (currentDocumentExt)
                {
                    case ".txt":
                        LoadTextDocument(filePath);
                        break;
                    case ".rtf":
                        LoadRtfDocument(filePath);
                        break;
                    case ".pdf":
                        LoadPdfDocumentAsync(filePath, loadingCts.Token);
                        break;
                    case ".doc":
                    case ".docx":
                        LoadWordDocument(filePath, loadingCts.Token);
                        break;
                    default:
                        LoadUnsupportedDocument(filePath);
                        break;
                }
                return unit;
            }).IfFail(ex =>
            {
                Debug.WriteLine($"Error loading document: {ex.Message}");
                ShowErrorMessage($"Cannot display this document: {ex.Message}");
                return unit;
            });
        }

        /// <summary>
        /// Loads and displays a plain text document
        /// </summary>
        void LoadTextDocument(string filePath) =>
            Try.lift(() =>
            {
                // Read text file with appropriate encoding detection
                var text = File.ReadAllText(filePath, Encoding.UTF8);

                SafeInvokeOnUIThread(() =>
                {
                    txtDocumentContent.Text = text;
                    txtDocumentContent.Visible = true;
                    rtfDocumentContent.Visible = false;
                    loadingPanel.Visible = false;
                });
                return unit;
            }).IfFail(ex =>
            {
                Debug.WriteLine($"Error loading text document: {ex.Message}");
                ShowErrorMessage($"Error loading text file: {ex.Message}");
                return unit;
            });

        /// <summary>
        /// Loads and displays an RTF document
        /// </summary>
        void LoadRtfDocument(string filePath) =>
            Try.lift(() =>
            {
                SafeInvokeOnUIThread(() =>
                {
                    rtfDocumentContent.LoadFile(filePath);
                    rtfDocumentContent.Visible = true;
                    txtDocumentContent.Visible = false;
                    loadingPanel.Visible = false;
                });
                return unit;
            }).IfFail(ex =>
            {
                Debug.WriteLine($"Error loading RTF document: {ex.Message}");
                ShowErrorMessage($"Error loading RTF file: {ex.Message}");
                return unit;
            });

        /// <summary>
        /// Loads a PDF document using IronPDF with cancellation support
        /// // todo do we need the cancellation support
        /// </summary>
        async void LoadPdfDocumentAsync(string filePath, CancellationToken cancellationToken) =>
            await Try.lift(async () =>
            {
                // Add a timeout for loading the PDF
                var loadPdfTask = Task.Run(() => new PdfDocument(filePath), cancellationToken);

                // Wait for the task to complete with a timeout
                if (await Task.WhenAny(loadPdfTask, Task.Delay(15000, cancellationToken)) != loadPdfTask)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        throw new TimeoutException("PDF loading timed out after 15 seconds");
                    }
                    return unit;
                }

                if (cancellationToken.IsCancellationRequested) return unit;

                // Get the loaded PDF document
                currentPdfDocument = await loadPdfTask;
                totalPdfPages = currentPdfDocument.PageCount;

                // Add a timeout for rendering the PDF
                var renderTask = Task.Run(() => currentPdfDocument.ToBitmap(150), cancellationToken);

                if (await Task.WhenAny(renderTask, Task.Delay(15000, cancellationToken)) != renderTask)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        throw new TimeoutException("PDF rendering timed out after 15 seconds");
                    }
                    return unit;
                }

                if (cancellationToken.IsCancellationRequested) return unit;

                // Get the rendered images
                var images = await renderTask;
                var pages = new List<System.Drawing.Image>();

                foreach (var img in images)
                {
                    if (cancellationToken.IsCancellationRequested) return unit;
                    pages.Add(img);
                }

                pdfPageImages = pages.ToArray();

                // Show the first page
                currentPdfPage = 0;

                SafeInvokeOnUIThread(() =>
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        ShowPdfPage(0);
                        pdfPanel.Visible = true;
                        pdfNavigationPanel.Visible = true;
                        loadingPanel.Visible = false;
                        Debug.WriteLine("PDF loaded successfully with IronPDF");
                    }
                });
                return unit;
            }).IfFail(ex =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    Debug.WriteLine($"Error rendering PDF with IronPDF: {ex.Message}");
                    SafeInvokeOnUIThread(() =>
                    {
                        ShowErrorMessage($"Error rendering PDF: {ex.Message}");
                        ShowOpenFileUI(filePath);
                    });
                }
                return Task.FromResult(unit);
            });               
           
        
        /// <summary>
        /// Loads and displays a Word document by converting it to PDF using IronPDF
        /// </summary>
        async void LoadWordDocument(string filePath, CancellationToken cancellationToken) =>
        
            await Try.lift(async () =>
            {
                SafeInvokeOnUIThread(() =>
                {
                    loadingLabel.Text = "Converting Word document...";
                });

                
                        // Create all resources locally within this task
                        var renderer = new DocxToPdfRenderer();

                        // Add a timeout for the Word to PDF conversion
                        var convertTask = Task.Run(() => renderer.RenderDocxAsPdf(filePath), cancellationToken);

                        // Wait for the task to complete with a timeout
                        if (await Task.WhenAny(convertTask, Task.Delay(30000, cancellationToken)) != convertTask)
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                throw new TimeoutException("Word document conversion timed out after 30 seconds");
                            }
                            return unit;
                        }

                        if (cancellationToken.IsCancellationRequested) return unit;

                        // Get the converted PDF
                        var pdf = await convertTask;

                        // Get page count
                        var pageCount = pdf.PageCount;

                        // Add a timeout for converting to bitmaps
                        var renderTask = Task.Run(() => pdf.ToBitmap(150), cancellationToken);

                        if (await Task.WhenAny(renderTask, Task.Delay(15000, cancellationToken)) != renderTask)
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                throw new TimeoutException("PDF rendering timed out after 15 seconds");
                            }
                            return unit;
                        }

                        if (cancellationToken.IsCancellationRequested) return unit;

                        // Get the rendered images
                        var images = await renderTask;
                        var pages = new List<System.Drawing.Image>();

                        foreach (var img in images)
                        {
                            if (cancellationToken.IsCancellationRequested) return unit;
                            pages.Add(img);
                        }

                        var pageImages = pages.ToArray();

                        // Only once everything is ready, update UI
                        SafeInvokeOnUIThread(() =>
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                pdfPageImages = pageImages;
                                totalPdfPages = pageCount;
                                currentPdfPage = 0;
                                ShowPdfPage(0);
                                pdfPanel.Visible = true;
                                pdfNavigationPanel.Visible = true;
                                loadingPanel.Visible = false;
                                Debug.WriteLine("Word document converted and loaded successfully");
                            }
                        });
                        return unit;
                    }).IfFail(ex =>
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            Debug.WriteLine($"Word document conversion failed: {ex.Message}");
                            SafeInvokeOnUIThread(() =>
                            {
                                ShowOpenFileUI(filePath);
                                CleanupPdfResources();
                                ShowOpenFileUI(filePath);
                            });
                        }
                        return Task.FromResult(unit);
                    });


        /// <summary>
        /// Creates a panel with button to open Word document externally
        /// </summary>
        void ShowOpenFileUI(string filePath) =>
            SafeInvokeOnUIThread(() =>
            {
                Try.lift(() =>
                {
                    // Create a simple panel with a button to open Word doc
                    Panel wordPanel = new Panel
                    {
                        Dock = DockStyle.Fill
                    };
                    ThemeManager.StylePanel(wordPanel, ThemeManager.PrimaryBackColor);

                    Label infoLabel = new Label
                    {
                        Text = $"Document: {Path.GetFileName(filePath)}",
                        AutoSize = false,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        Height = 40
                    };
                    ThemeManager.StyleLabel(infoLabel, ThemeManager.HeaderFont);

                    Label errorLabel = new Label
                    {
                        Text = "Unable to render document in viewer. You can open it externally:",
                        AutoSize = false,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        Height = 40
                    };
                    ThemeManager.StyleLabel(errorLabel);

                    Button openButton = new Button
                    {
                        Text = "Open",
                        Width = 250,
                        Height = 60,
                        Anchor = AnchorStyles.None
                    };
                    ThemeManager.StyleButton(openButton);

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
                    return unit;
                }).IfFail(ex =>
                {
                    Debug.WriteLine($"Error creating open file UI: {ex.Message}");
                    return unit;
                });
            });        

        /// <summary>
        /// Displays a specific page of the PDF
        /// </summary>
        private void ShowPdfPage(int pageIndex) =>
            Try.lift(() =>
            {
                if (pageIndex < 0 || pageIndex >= pdfPageImages.Length)
                {
                    pageIndex = 0;
                }

                // Clean up old image if there is one
                if (pdfPictureBox.Image != null && pdfPictureBox.Image != pdfPageImages[pageIndex])
                {
                    // Don't dispose the image as we're keeping them in the pdfPageImages list
                    pdfPictureBox.Image = null;
                }

                // Show the new page
                pdfPictureBox.Image = pdfPageImages[pageIndex];
                currentPdfPage = pageIndex;
                lblPageInfo.Text = $"Page: {pageIndex + 1}/{totalPdfPages}";

                // Update button states
                btnPrevPage.Enabled = pageIndex > 0;
                btnNextPage.Enabled = pageIndex < totalPdfPages - 1;
                return unit;
            }).IfFail(ex =>
            {
                Debug.WriteLine($"Error showing PDF page: {ex.Message}");
                return unit;
            });

        /// <summary>
        /// Opens a document in its associated external application
        /// </summary>
        private Unit OpenDocumentExternally(string filePath) =>
            Try.lift(() =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
                return unit;
            }).IfFail(ex =>
            {
                Debug.WriteLine($"Error opening file externally: {ex.Message}");
                MessageBox.Show($"Could not open the document: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return unit;
            });

        /// <summary>
        /// Shows a message for unsupported document types
        /// </summary>
        void LoadUnsupportedDocument(string filePath)
        {
            ShowErrorMessage($"Preview not available for {Path.GetExtension(filePath).ToUpper()} files.");

            SafeInvokeOnUIThread(() =>
            {
                loadingPanel.Visible = false;
            });
        }

        /// <summary>
        /// Displays an error message in the text viewer
        /// </summary>
        void ShowErrorMessage(string message)
        {
            Debug.WriteLine($"Document viewer error: {message}");

            SafeInvokeOnUIThread(() =>
            {
                pdfPanel.Visible = false;
                pdfNavigationPanel.Visible = false;
                loadingPanel.Visible = false;
                rtfDocumentContent.Visible = false;
                txtDocumentContent.Text = message;
                txtDocumentContent.Visible = true;
            });
        }
        /// <summary>
        /// Resets all viewers to hidden state
        /// </summary>
        void ResetViewers() =>
            SafeInvokeOnUIThread(() =>
            {
                Try.lift(() =>
                {
                    // Hide existing viewers
                    txtDocumentContent.Visible = false;
                    txtDocumentContent.Text = string.Empty;
                    rtfDocumentContent.Visible = false;
                    rtfDocumentContent.Clear();
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
                            control != rtfDocumentContent &&
                            control != pdfPanel &&
                            control != loadingPanel &&
                            control != pdfNavigationPanel)
                        {
                            Controls.RemoveAt(i);
                            control.Dispose();
                        }
                    }
                    return unit;
                }).IfFail(ex =>
                {
                    Debug.WriteLine($"Error resetting viewers: {ex.Message}");
                    return unit;
                });
            });

        /// <summary>
        /// Clean up PDF resources
        /// </summary>
        private Unit CleanupPdfResources() =>
            Try.lift(() =>
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
                    Try.lift(() =>
                    {
                        File.Delete(tempPdfPath);
                        tempPdfPath = null;
                        return unit;
                    }).IfFail(ex =>
                    {
                        Debug.WriteLine($"Error deleting temporary PDF: {ex.Message}");
                        return unit;
                    });
                }

                // Reset page counters
                currentPdfPage = 0;
                totalPdfPages = 0;
                return unit;
            }).IfFail(ex =>
            {
                Debug.WriteLine($"Error cleaning up PDF resources: {ex.Message}");
                return unit;
            });

        /// <summary>
        /// Clean up resources
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Try.lift(() =>
                {
                    Debug.WriteLine("Disposing DocumentViewerControl");

                    // Cancel any active loading operations
                    loadingCts?.Cancel();
                    loadingCts?.Dispose();
                    loadingCts = null;

                    // Close any open documents
                    ResetViewers();

                    // Clean up PDF resources
                    CleanupPdfResources();

                    // Dispose of controls
                    txtDocumentContent?.Dispose();
                    rtfDocumentContent?.Dispose();
                    loadingPanel?.Dispose();
                    loadingLabel?.Dispose();
                    pdfPanel?.Dispose();
                    pdfPictureBox?.Dispose();
                    pdfNavigationPanel?.Dispose();
                    btnPrevPage?.Dispose();
                    btnNextPage?.Dispose();
                    lblPageInfo?.Dispose();
                    return unit;
                }).IfFail(ex =>
                {
                    Debug.WriteLine($"Error disposing document viewer: {ex.Message}");
                    return unit;
                });
            }

            base.Dispose(disposing);
        }
    }
}