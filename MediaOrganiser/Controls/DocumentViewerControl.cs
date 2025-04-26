using LanguageExt;
using System.Diagnostics;
using System.IO;
using System.Text;
using Image = System.Drawing.Image;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using static LanguageExt.Prelude;

namespace MediaOrganiser.Controls;

/// <summary>
/// A document viewer control with integrated PDF display using IronPDF
/// and Word document rendering via conversion to PDF
/// </summary>
public class DocumentViewerControl : UserControl
{
    /// <summary>
    /// UI components
    /// </summary>
    TextBox txtDocumentContent = new ();
    RichTextBox rtfDocumentContent = new ();
    Panel loadingPanel = new ();
    Label loadingLabel = new ();
    Panel pdfPanel = new ();
    PictureBox pdfPictureBox = new ();
    Panel pdfNavigationPanel = new ();
    Button btnPrevPage = new ();
    Button btnNextPage = new ();
    Label lblPageInfo = new ();

    /// <summary>
    /// PDF rendering components
    /// </summary>
    Option<PdfDocument> currentPdfDocument;
    Image[] pdfPageImages = [];
    int currentPdfPage = 0;
    int totalPdfPages = 0;

    /// <summary>
    /// Temp file for word to pdf conversion
    /// </summary>
    Option<string> tempPdfPath;

    /// <summary>
    /// Sync context for UI operations
    /// </summary>
    readonly SynchronizationContext syncContext = new ();

    /// <summary>
    /// Cancellation token source for document loading operations
    /// </summary>
    CancellationTokenSource loadingCts = new ();

    /// <summary>
    /// Creates and initializes the document viewer control
    /// </summary>
    public DocumentViewerControl()
    {
        syncContext = SynchronizationContext.Current ?? new ();
        InitializeComponents();
    }

    /// <summary>
    /// Initializes all UI components for document viewing
    /// </summary>
    void InitializeComponents()
    {
        // Plain text content (eg txt files)
        txtDocumentContent = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            Visible = false
        };
        ThemeManager.StyleTextBox(txtDocumentContent);

        // Rich text viewer for RTF documents
        rtfDocumentContent = new RichTextBox
        {
            ReadOnly = true,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            Dock = DockStyle.Fill,
            Visible = false
        };
        ThemeManager.StyleRichTextBox(rtfDocumentContent);

        // Loading indicator
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

        // Pdf navigation panel, matches main form style
        pdfNavigationPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 58, 
            Visible = false,
            BackColor = ThemeManager.SecondaryBackColor,
            Padding = new Padding(6, 6, 6, 6)
        };

        btnPrevPage = new Button
        {
            Text = "Back",
            Size = new Size(130, 45), 
            Enabled = false,
            Location = new Point(4, 4),
            Margin = new Padding(4, 4, 4, 4), 
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Top
        };

        btnNextPage = new Button
        {
            Text = "Next",
            Size = new Size(130, 45), 
            Enabled = false,
            Location = new Point(150, 4),
            Margin = new Padding(4, 4, 4, 4), 
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Top
        };

        ThemeManager.StyleSecondaryButton(btnPrevPage);
        ThemeManager.StyleSecondaryButton(btnNextPage);

        lblPageInfo = new Label
        {
            Text = "Page: 0 / 0",
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(290, 13), 
            ForeColor = Color.Black,
            Anchor = AnchorStyles.Left | AnchorStyles.Top
        };
        ThemeManager.StyleLabel(lblPageInfo);

        btnPrevPage.Click += (s, e) => ShowPdfPage(currentPdfPage - 1);
        btnNextPage.Click += (s, e) => ShowPdfPage(currentPdfPage + 1);

        pdfNavigationPanel.Controls.Add(lblPageInfo);
        pdfNavigationPanel.Controls.Add(btnNextPage);
        pdfNavigationPanel.Controls.Add(btnPrevPage);

        // Panel for PDF display
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
            Visible = true,
            BackColor = ThemeManager.PrimaryBackColor
        };

        pdfPanel.Controls.Add(pdfPictureBox);

        // Initialize the IronPdf license if you have one, otherwise you get a watermark
        InitializeIronPdf();

        Controls.Add(txtDocumentContent);
        Controls.Add(rtfDocumentContent);
        Controls.Add(loadingPanel);
        Controls.Add(pdfPanel);
        Controls.Add(pdfNavigationPanel);
    }

    /// <summary>
    /// Initialize IronPdf library
    /// </summary>
    static Unit InitializeIronPdf() =>
        Try.lift(() =>
        {
            // If you have a license, uncomment this line and add your license key
            // IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

            // Configure rendering
            Installation.TempFolderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MediaOrganiser", "IronPdfTemp");

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
    Unit SafeInvokeOnUIThread(Action action) =>
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
            loadingCts?.Cancel();
            loadingCts = new CancellationTokenSource();

            var currentDocumentExt = Path.GetExtension(filePath).ToLower();

            if (!File.Exists(filePath))
            {
                ShowErrorMessage($"File not found: {filePath}");
                return unit;
            }

            SafeInvokeOnUIThread(() =>
            {
                loadingPanel.Visible = true;
            });

            switch (currentDocumentExt)
            {
                case ".txt":
                    LoadTextDocument(filePath);
                    break;
                case ".rtf":
                    LoadRtfDocument(filePath);
                    break;
                case ".pdf":
                    LoadPdfDocumentAsync(filePath);
                    break;
                case ".doc":
                case ".docx":
                    LoadWordDocument(filePath);
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
    /// </summary>
    async void LoadPdfDocumentAsync(string filePath) =>
        await Try.lift(async () =>
        {
            var loadPdfTask = Task.Run(() => new PdfDocument(filePath), loadingCts.Token);

            // Wait for the task to complete with a timeout
            if (await Task.WhenAny(loadPdfTask, Task.Delay(15000, loadingCts.Token)) != loadPdfTask)
            {
                if (!loadingCts.Token.IsCancellationRequested)
                {
                    throw new TimeoutException("PDF loading timed out after 15 seconds");
                }
                return unit;
            }

            if (loadingCts.Token.IsCancellationRequested) return unit;

            var pdf = await loadPdfTask;
            totalPdfPages = currentPdfDocument.Map(t => t.PageCount).IfNone(0);
            currentPdfDocument = pdf;

            // Add a timeout for rendering the PDF
            var renderTask = Task.Run(() => pdf.ToBitmap(150), loadingCts.Token);

            if (await Task.WhenAny(renderTask, Task.Delay(15000, loadingCts.Token)) != renderTask)
            {
                if (!loadingCts.Token.IsCancellationRequested)
                {
                    throw new TimeoutException("PDF rendering timed out after 15 seconds");
                }
                return unit;
            }

            if (loadingCts.Token.IsCancellationRequested) return unit;

            // Get the rendered images
            var images = await renderTask;
            var pages = new List<System.Drawing.Image>();

            foreach (var img in images)
            {
                if (loadingCts.Token.IsCancellationRequested) return unit;
                pages.Add(img);
            }

            pdfPageImages = [..pages];

            currentPdfPage = 0;

            SafeInvokeOnUIThread(() =>
            {
                if (!loadingCts.Token.IsCancellationRequested)
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
            if (!loadingCts.Token.IsCancellationRequested)
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
    async void LoadWordDocument(string filePath) =>
        await Try.lift(async () =>
        {
            var cancellationToken = loadingCts.Token;

            SafeInvokeOnUIThread(() =>
            {
                loadingLabel.Text = "Converting Word document...";
            });

            var renderer = new DocxToPdfRenderer();
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
            if (!loadingCts.Token.IsCancellationRequested)
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
    /// This is an 'if all else fails' operation
    /// </summary>
    void ShowOpenFileUI(string filePath) =>
        SafeInvokeOnUIThread(() =>
        {
            Try.lift(() =>
            {
                var wordPanel = new Panel()
                {
                    Dock = DockStyle.Fill
                };
                ThemeManager.StylePanel(wordPanel, ThemeManager.PrimaryBackColor);

                var infoLabel = new Label
                {
                    Text = $"Document: {Path.GetFileName(filePath)}",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 40
                };
                ThemeManager.StyleLabel(infoLabel, ThemeManager.HeaderFont);

                var errorLabel = new Label
                {
                    Text = "Unable to render document in viewer. You can open it externally:",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 40
                };
                ThemeManager.StyleLabel(errorLabel);

                var openButton = new Button
                {
                    Text = "Open",
                    Width = 250,
                    Height = 60,
                    Anchor = AnchorStyles.None
                };
                ThemeManager.StyleButton(openButton);

                openButton.Click += (s, e) => OpenDocumentExternally(filePath);

                // Center
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
    void ShowPdfPage(int pageIndex) =>
        Try.lift(() =>
        {
            if (pageIndex < 0 || pageIndex >= pdfPageImages.Length)
            {
                pageIndex = 0;
            }

            // Clean up old image if there is one
            if (pdfPictureBox.Image != null && pdfPictureBox.Image != pdfPageImages[pageIndex])
            {
                // Don't dispose.. handled separately by the dispose
                pdfPictureBox.Image = null;
            }

            pdfPictureBox.Image = pdfPageImages[pageIndex];
            currentPdfPage = pageIndex;
            lblPageInfo.Text = $"Page: {pageIndex + 1}/{totalPdfPages}";

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
    static Unit OpenDocumentExternally(string filePath) =>
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
    void ShowErrorMessage(string message) =>    
        SafeInvokeOnUIThread(() =>
        {
            pdfPanel.Visible = false;
            pdfNavigationPanel.Visible = false;
            loadingPanel.Visible = false;
            rtfDocumentContent.Visible = false;
            txtDocumentContent.Text = message;
            txtDocumentContent.Visible = true;
        });
    
    /// <summary>
    /// Resets all viewers to hidden state
    /// </summary>
    void ResetViewers() =>
        SafeInvokeOnUIThread(() =>
        {
            Try.lift(() =>
            {
                txtDocumentContent.Visible = false;
                txtDocumentContent.Text = string.Empty;
                rtfDocumentContent.Visible = false;
                rtfDocumentContent.Clear();
                pdfPanel.Visible = false;
                pdfNavigationPanel.Visible = false;

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
    Unit CleanupPdfResources() =>
        Try.lift(() =>
        {
            // Clean up PDF page images
            if (pdfPageImages != null)
            {
                foreach (var image in pdfPageImages)
                {
                    image?.Dispose();
                }
                pdfPageImages = [];
            }

            currentPdfDocument.IfSome(p => p.Dispose());
            currentPdfDocument = None;

            tempPdfPath.IfSome(t =>
            {
                Try.lift(() =>
                {
                    if (File.Exists(t))
                    {
                        File.Delete(t);
                    }
                    return unit;
                }).IfFail(ex =>
                {
                    Debug.WriteLine($"Error deleting temporary PDF: {ex.Message}");
                    return unit;
                });

                tempPdfPath = None;
            });           

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

                ResetViewers();
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