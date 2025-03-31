using LanguageExt;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static MediaOrganiser.Core.AppErrors;

namespace MediaOrganiser
{
    /// <summary>
    /// Form to display a list of errors in a scrollable listbox
    /// </summary>
    public partial class ErrorListForm : Form
    {
        private ListBox listErrors;
        private Button btnDismiss;
        private Label lblTitle;
        private Panel pnlContent;

        public ErrorListForm(string title, Seq<UserError> errors)
        {
            InitializeComponents();

            // Set the form title
            lblTitle.Text = title;

            // Add errors to listbox
            foreach (var error in errors)
                listErrors.Items.Add(error.Message);

            // Apply theme
            ThemeManager.ApplyTheme(this);
            ThemeManager.StylePrimaryButton(btnDismiss);
        }

        /// <summary>
        /// Initializes form components
        /// </summary>
        private void InitializeComponents()
        {
            // Set up form properties
            Text = "Errors";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = ThemeManager.FormBackgroundColor;

            // Set form size to be larger to accommodate more error text
            Size = new System.Drawing.Size(1500, 800);

            // Create title label
            lblTitle = new Label
            {
                Text = "Errors",
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };
            ThemeManager.StyleLabel(lblTitle, ThemeManager.HeaderFont);

            // Create content panel to hold list and button
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };
            ThemeManager.StylePanel(pnlContent);

            // Create error listbox with increased font size for better readability
            listErrors = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false,
                Font = new System.Drawing.Font(ThemeManager.DefaultFont.FontFamily,
                                             ThemeManager.DefaultFont.Size + 1,
                                             ThemeManager.DefaultFont.Style)
            };

            // Create dismiss button with increased height
            btnDismiss = new Button
            {
                Text = "Dismiss",
                Dock = DockStyle.Bottom,
                Height = 60,
                Margin = new Padding(0, 15, 0, 0)
            };

            // Handle dismiss button click
            btnDismiss.Click += (s, e) => Close();

            // Add controls to form
            pnlContent.Controls.Add(listErrors);
            pnlContent.Controls.Add(btnDismiss);
            Controls.Add(pnlContent);
            Controls.Add(lblTitle);

            // Set accept button
            AcceptButton = btnDismiss;
        }

        /// <summary>
        /// Shows the error list form as a dialog
        /// </summary>
        public static void ShowErrors(Form owner, string title, Seq<UserError> errors)
        {
            using var form = new ErrorListForm(title, errors);
            form.ShowDialog(owner);
        }
    }
}