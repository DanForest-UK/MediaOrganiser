using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Drawing.Drawing2D;
using Size = System.Drawing.Size;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace MediaOrganiser
{
    public static class AppStyling
    {
        // Define a color palette for the application
        public static class AppColors
        {
            public static Color Primary = Color.FromArgb(52, 152, 219);       // Blue
            public static Color Secondary = Color.FromArgb(41, 128, 185);     // Darker blue
            public static Color Accent = Color.FromArgb(46, 204, 113);        // Green
            public static Color Warning = Color.FromArgb(231, 76, 60);        // Red
            public static Color Background = Color.FromArgb(236, 240, 241);   // Light gray
            public static Color DarkBackground = Color.FromArgb(52, 73, 94);  // Dark slate
            public static Color TextPrimary = Color.FromArgb(44, 62, 80);     // Dark text
            public static Color TextLight = Color.FromArgb(255, 255, 255);    // White text
        }

        /// <summary>
        /// Applies the app styling to the main form
        /// </summary>
        public static void ApplyToForm(Form1 form)
        {
            // Apply styles to form
            form.BackColor = AppColors.Background;
            form.ForeColor = AppColors.TextPrimary;

            // Style the navigation buttons
            StyleNavigationButtons(form);

            // Style action buttons (Bin/Keep)
            StyleActionButtons(form);

            // Style browsing elements
            StyleBrowseElements(form);

            // Style organise section
            StyleOrganiseSection(form);

            // Update status bar style
            form.lblStatus.ForeColor = AppColors.TextPrimary;

            // Image viewer background
            form.picCurrentImage.BackColor = Color.White;

            // Progress bar styling - make it more modern
            StyleProgressBar(form.progressScan);

            // Style the main panel that contains the buttons
            if (form.pnlButtons != null)
            {
                form.pnlButtons.BackColor = Color.Transparent;
            }

            // Set form padding for better spacing
            form.Padding = new Padding(10);

            // Apply custom fonts if desired
            ApplyFonts(form);
        }

        /// <summary>
        /// Applies custom fonts to key UI elements
        /// </summary>
        private static void ApplyFonts(Form1 form)
        {
            // Use system font for a cleaner look
            Font regularFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            Font boldFont = new Font("Segoe UI", 9F, FontStyle.Bold);

            // Apply fonts to key elements
            form.Font = regularFont;
            form.lblStatus.Font = regularFont;

            // Set button fonts
            SetButtonFont(form.btnBrowseFolder, regularFont);
            SetButtonFont(form.btnScanFiles, regularFont);
            SetButtonFont(form.btnOrganiseFiles, regularFont);
            SetButtonFont(form.btnPrevious, regularFont);
            SetButtonFont(form.btnNext, regularFont);
            SetButtonFont(form.btnBin, regularFont);
            SetButtonFont(form.btnKeep, regularFont);

            // Other controls
            form.switchCopyOnly.Font = regularFont;
            form.txtFolderPath.Font = regularFont;
        }

        /// <summary>
        /// Sets a font on a button if the button exists
        /// </summary>
        private static void SetButtonFont(Button button, Font font)
        {
            if (button != null)
            {
                button.Font = font;
            }
        }

        /// <summary>
        /// Styles the navigation buttons with better appearance
        /// </summary>
        private static void StyleNavigationButtons(Form1 form)
        {
            // Previous (Back) button styling
            StyleButton(
                form.btnPrevious,
                AppColors.Secondary,
                AppColors.TextLight,
                90, 35);

            // Next button styling
            StyleButton(
                form.btnNext,
                AppColors.Primary,
                AppColors.TextLight,
                90, 35);
        }

        /// <summary>
        /// Styles the action buttons (Bin/Keep)
        /// </summary>
        private static void StyleActionButtons(Form1 form)
        {
            // Bin button styling
            StyleButton(
                form.btnBin,
                AppColors.Warning,
                AppColors.TextLight,
                100, 40);

            // Keep button styling
            StyleButton(
                form.btnKeep,
                AppColors.Accent,
                AppColors.TextLight,
                100, 40);
        }

        /// <summary>
        /// Styles the browse elements
        /// </summary>
        private static void StyleBrowseElements(Form1 form)
        {
            // Browse button
            StyleButton(form.btnBrowseFolder, AppColors.DarkBackground, AppColors.TextLight);

            // Folder path textbox
            form.txtFolderPath.BorderStyle = BorderStyle.FixedSingle;
            form.txtFolderPath.BackColor = Color.White;

            // Scan files button
            StyleButton(form.btnScanFiles, AppColors.Primary, AppColors.TextLight);
        }

        /// <summary>
        /// Styles the organise section
        /// </summary>
        private static void StyleOrganiseSection(Form1 form)
        {
            // Organise button
            StyleButton(form.btnOrganiseFiles, AppColors.Primary, AppColors.TextLight);

            // Copy only checkbox
            form.switchCopyOnly.ForeColor = AppColors.TextPrimary;
        }

        /// <summary>
        /// Styles a button with text
        /// </summary>
        private static void StyleButton(Button button, Color backColor, Color foreColor, int width = 150, int height = 46)
        {
            if (button == null) return;

            // Set button properties
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Size = new Size(width, height);
            button.Cursor = Cursors.Hand;

            // Round the corners (Windows Forms doesn't directly support this, but we can set a Region)
            // This has to be done after controls are fully initialized and have proper size
            button.HandleCreated += (s, e) => RoundCorners(button, 5);
        }

        /// <summary>
        /// Makes a control have rounded corners
        /// </summary>
        private static void RoundCorners(Control control, int radius)
        {
            try
            {
                Rectangle rect = new Rectangle(0, 0, control.Width, control.Height);
                GraphicsPath path = RoundedRect(rect, radius);
                control.Region = new Region(path);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error rounding corners: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a rounded rectangle path
        /// </summary>
        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // Top left arc  
            path.AddArc(arc, 180, 90);

            // Top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Styles a progress bar to look more modern
        /// </summary>
        private static void StyleProgressBar(ProgressBar progressBar)
        {
            if (progressBar == null) return;

            progressBar.BackColor = Color.White;
            progressBar.ForeColor = AppColors.Accent;
        }
    }
}