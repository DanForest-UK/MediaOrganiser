using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MediaOrganiser
{
    /// <summary>
    /// A control for displaying document content (PDF, Word, text)
    /// </summary>
    public class DocumentViewerControl : UserControl
    {
        // UI components
        TextBox txtDocumentContent;
        Panel pdfViewerPanel;
        WebBrowser webBrowser;

        // Current document path
        string currentDocumentPath;
        string currentDocumentExt;

        /// <summary>
        /// Creates and initializes the document viewer control
        /// </summary>
        public DocumentViewerControl()
        {
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

            // Create web browser for PDF and possibly Word docs
            webBrowser = new WebBrowser
            {
                Dock = DockStyle.Fill,
                Visible = false
            };

            // Panel for PDF viewer (can be replaced with specialized viewers)
            pdfViewerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false
            };

            // Add controls to this control
            Controls.Add(txtDocumentContent);
            Controls.Add(webBrowser);
            Controls.Add(pdfViewerPanel);
        }

        /// <summary>
        /// Loads and displays a document based on its file extension
        /// </summary>
        public void LoadDocument(string filePath)
        {
            try
            {
                // Reset state
                ResetViewers();

                currentDocumentPath = filePath;
                currentDocumentExt = Path.GetExtension(filePath).ToLower();

                switch (currentDocumentExt)
                {
                    case ".txt":
                        LoadTextDocument(filePath);
                        break;
                    case ".pdf":
                        LoadPdfDocument(filePath);
                        break;
                    case ".doc":
                    case ".docx":
                        LoadWordDocument(filePath);
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
            // Read text file with appropriate encoding detection
            var text = File.ReadAllText(filePath, Encoding.UTF8);
            txtDocumentContent.Text = text;
            txtDocumentContent.Visible = true;
        }

        /// <summary>
        /// Loads and displays a PDF document
        /// </summary>
        void LoadPdfDocument(string filePath)
        {
            try
            {
                // Basic approach: use web browser with PDF reader
                webBrowser.Navigate(filePath);
                webBrowser.Visible = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PDF loading error: {ex.Message}");
                ShowErrorMessage("PDF preview not available. You may need Adobe Reader or another PDF viewer installed.");
            }
        }

        /// <summary>
        /// Loads and displays a Word document
        /// </summary>
        void LoadWordDocument(string filePath)
        {
            try
            {
                // Basic approach: For now, just use web browser's limited capabilities
                // A more advanced approach would use a COM object or specific library
                webBrowser.Navigate(filePath);
                webBrowser.Visible = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Word document loading error: {ex.Message}");
                ShowErrorMessage("Word document preview not available. You may need Microsoft Word installed.");
            }
        }

        /// <summary>
        /// Shows a message for unsupported document types
        /// </summary>
        void LoadUnsupportedDocument(string filePath)
        {
            ShowErrorMessage($"Preview not available for {Path.GetExtension(filePath).ToUpper()} files.");
        }

        /// <summary>
        /// Displays an error message in the text viewer
        /// </summary>
        void ShowErrorMessage(string message)
        {
            txtDocumentContent.Text = message;
            txtDocumentContent.Visible = true;
        }

        /// <summary>
        /// Resets all viewers to hidden state
        /// </summary>
        void ResetViewers()
        {
            txtDocumentContent.Visible = false;
            txtDocumentContent.Text = string.Empty;

            webBrowser.Visible = false;
            pdfViewerPanel.Visible = false;
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
                    // Close any open documents
                    ResetViewers();

                    // Dispose of controls
                    webBrowser?.Dispose();
                    txtDocumentContent?.Dispose();
                    pdfViewerPanel?.Dispose();
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