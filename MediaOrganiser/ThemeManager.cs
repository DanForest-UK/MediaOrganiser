using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace MediaOrganiser;

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
    public static readonly Color PrimaryBackColor = Color.FromArgb(250, 250, 250);   // Almost white 
    public static readonly Color SecondaryBackColor = Color.FromArgb(240, 242, 245); // Light gray 
    public static readonly Color AccentBackColor = Color.FromArgb(228, 230, 235);    // Slightly darker gray 
    public static readonly Color ControlBackColor = Color.FromArgb(245, 246, 247);   // Light gray

    public static readonly Color PrimaryTextColor = Color.FromArgb(33, 37, 41);      // Dark gray 
    public static readonly Color SecondaryTextColor = Color.FromArgb(108, 117, 125); // Medium gray 

    public static readonly Color AccentColor = Color.FromArgb(13, 110, 253);         // Blue 
    public static readonly Color SuccessColor = Color.FromArgb(25, 135, 84);         // Green 
    public static readonly Color DangerColor = Color.FromArgb(220, 53, 69);          // Red
    public static readonly Color SecondaryColor = Color.FromArgb(108, 117, 125);     // Gray

    public static readonly Color ButtonBackColor = Color.FromArgb(248, 249, 250);    // Light
    public static readonly Color ButtonHoverColor = Color.FromArgb(235, 236, 240);   // Slightly darker
    public static readonly Color ButtonTextColor = Color.FromArgb(33, 37, 41);       // Dark 

    // Form background color
    public static readonly Color FormBackgroundColor = Color.FromArgb(236, 240, 243); // Blue light gray

    /// <summary>
    /// Applies standard styling to a button control.
    /// </summary>
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

    /// <summary>
    /// Applies standard styling to a label control with optional custom font.
    /// </summary>
    public static Label StyleLabel(Label label, Font font = null)
    {
        label.Font = font ?? DefaultFont;
        label.ForeColor = PrimaryTextColor;
        return label;
    }

    /// <summary>
    /// Applies standard styling to a text box control.
    /// </summary>
    public static TextBox StyleTextBox(TextBox textBox)
    {
        textBox.Font = DefaultFont;
        textBox.BackColor = PrimaryBackColor;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        return textBox;
    }

    /// <summary>
    /// Applies standard styling to a rich text box control.
    /// </summary>
    public static RichTextBox StyleRichTextBox(RichTextBox rtfBox)
    {
        rtfBox.Font = DefaultFont;
        rtfBox.BackColor = PrimaryBackColor;
        rtfBox.BorderStyle = BorderStyle.FixedSingle;
        return rtfBox;
    }

    /// <summary>
    /// Applies standard styling to a panel control with optional custom background color.
    /// </summary>
    public static Panel StylePanel(Panel panel, Color? backColor = null)
    {
        panel.BackColor = backColor ?? SecondaryBackColor;
        return panel;
    }

    /// <summary>
    /// Applies the application theme to an entire form and all its controls.
    /// </summary>
    public static void ApplyTheme(Form form)
    {
        form.Font = DefaultFont;
        form.BackColor = FormBackgroundColor;
        ApplyThemeToControls(form.Controls);
    }

    /// <summary>
    /// Recursively applies the theme to all controls in a collection.
    /// </summary>
    private static void ApplyThemeToControls(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
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
                case RichTextBox richTextBox:
                    StyleRichTextBox(richTextBox);
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
            }

            // Recursively apply to children
            if (control.Controls.Count > 0)
            {
                ApplyThemeToControls(control.Controls);
            }
        }
    }

    /// <summary>
    /// Applies the application theme to a user control and all its child controls.
    /// </summary>
    public static void ApplyTheme(UserControl userControl)
    {
        userControl.Font = DefaultFont;
        userControl.BackColor = ControlBackColor;
        ApplyThemeToControls(userControl.Controls);
    }

    /// <summary>
    /// Styles a button with success theme (green) 
    /// </summary>
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

    /// <summary>
    /// Styles a button with danger theme (red) 
    /// </summary>
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

    /// <summary>
    /// Styles a button with primary theme (blue) 
    /// </summary>
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

    /// <summary>
    /// Styles a button with secondary theme (gray)
    /// </summary>
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