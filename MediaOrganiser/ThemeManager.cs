using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace MediaOrganiser
{
    /// <summary>
    /// Manages application-wide styling and theming
    /// </summary>
    public static class ThemeManager
    {
        // Font definitions
        public static readonly Font DefaultFont = new Font("Segoe UI", 9F, FontStyle.Regular);
        public static readonly Font HeaderFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        public static readonly Font SubHeaderFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font SmallFont = new Font("Segoe UI", 8F, FontStyle.Regular);

        // Modern color palette
        public static readonly Color PrimaryBackColor = Color.FromArgb(250, 250, 250);   // Almost white for backgrounds
        public static readonly Color SecondaryBackColor = Color.FromArgb(240, 242, 245); // Light gray for secondary backgrounds
        public static readonly Color AccentBackColor = Color.FromArgb(228, 230, 235);    // Slightly darker gray for accents
        public static readonly Color ControlBackColor = Color.FromArgb(245, 246, 247);   // Very light gray for controls

        public static readonly Color PrimaryTextColor = Color.FromArgb(33, 37, 41);      // Dark gray for primary text
        public static readonly Color SecondaryTextColor = Color.FromArgb(108, 117, 125); // Medium gray for secondary text

        public static readonly Color AccentColor = Color.FromArgb(13, 110, 253);         // Primary blue accent
        public static readonly Color SuccessColor = Color.FromArgb(25, 135, 84);         // Success green 
        public static readonly Color DangerColor = Color.FromArgb(220, 53, 69);          // Danger red
        public static readonly Color SecondaryColor = Color.FromArgb(108, 117, 125);     // Secondary gray

        public static readonly Color ButtonBackColor = Color.FromArgb(248, 249, 250);    // Light for buttons
        public static readonly Color ButtonHoverColor = Color.FromArgb(235, 236, 240);   // Slightly darker for hover
        public static readonly Color ButtonTextColor = Color.FromArgb(33, 37, 41);       // Dark text on buttons

        // Form background color
        public static readonly Color FormBackgroundColor = Color.FromArgb(236, 240, 243); // Slightly blue-tinted light gray

        // Button styling
        public static Button StyleButton(Button button)
        {
            button.Font = DefaultFont;
            button.BackColor = ButtonBackColor;
            button.ForeColor = ButtonTextColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = AccentBackColor;
            button.FlatAppearance.BorderSize = 1;
            return button;
        }

        // Label styling
        public static Label StyleLabel(Label label, Font font = null)
        {
            label.Font = font ?? DefaultFont;
            label.ForeColor = PrimaryTextColor;
            return label;
        }

        // Text box styling
        public static TextBox StyleTextBox(TextBox textBox)
        {
            textBox.Font = DefaultFont;
            textBox.BackColor = PrimaryBackColor;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            return textBox;
        }

        // Panel styling
        public static Panel StylePanel(Panel panel, Color? backColor = null)
        {
            panel.BackColor = backColor ?? SecondaryBackColor;
            return panel;
        }

        // Apply theme to an entire form
        public static void ApplyTheme(Form form)
        {
            form.Font = DefaultFont;
            form.BackColor = FormBackgroundColor;

            // Apply styles to all controls in the form
            ApplyThemeToControls(form.Controls);
        }

        // Recursively apply theme to all controls in a collection
        private static void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                // Apply styles based on control type
                switch (control)
                {
                    case Button button:
                        StyleButton(button);
                        break;
                    case Label label:
                        StyleLabel(label);
                        break;
                    case TextBox textBox:
                        StyleTextBox(textBox);
                        break;
                    case Panel panel:
                        StylePanel(panel);
                        break;
                    case PictureBox pictureBox:
                        pictureBox.BackColor = PrimaryBackColor;
                        break;
                    case CheckBox checkBox:
                        checkBox.ForeColor = PrimaryTextColor;
                        break;
                    case RadioButton radioButton:
                        radioButton.ForeColor = PrimaryTextColor;
                        break;
                    case ProgressBar progressBar:
                        // Not much we can do with progress bar in WinForms without custom drawing
                        break;
                }

                // Recursively apply to children
                if (control.Controls.Count > 0)
                {
                    ApplyThemeToControls(control.Controls);
                }
            }
        }

        // Apply theme to a specific user control
        public static void ApplyTheme(UserControl userControl)
        {
            userControl.Font = DefaultFont;
            userControl.BackColor = ControlBackColor;

            // Apply styles to all controls in the user control
            ApplyThemeToControls(userControl.Controls);
        }

        // Style success button (green for "Keep" actions)
        public static Button StyleSuccessButton(Button button)
        {
            button.BackColor = SuccessColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = DefaultFont;
            button.Cursor = Cursors.Hand;
            return button;
        }

        // Style danger button (red for "Bin" actions)
        public static Button StyleDangerButton(Button button)
        {
            button.BackColor = DangerColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = DefaultFont;
            button.Cursor = Cursors.Hand;
            return button;
        }

        // Style primary button (blue for main actions)
        public static Button StylePrimaryButton(Button button)
        {
            button.BackColor = AccentColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = DefaultFont;
            button.Cursor = Cursors.Hand;
            return button;
        }

        // Style secondary button (gray for navigation actions)
        public static Button StyleSecondaryButton(Button button)
        {
            button.BackColor = SecondaryColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = DefaultFont;
            button.Cursor = Cursors.Hand;
            return button;
        }
    }
}