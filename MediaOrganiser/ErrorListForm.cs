using LanguageExt;
using MediaOrganiser.Domain;

namespace MediaOrganiser;

/// <summary>
/// Form to display a list of errors in a scrollable listbox
/// </summary>
public partial class ErrorListForm : Form
{
    ListBox listErrors = new ();
    Button btnDismiss = new ();
    Label lblTitle = new ();
    Panel pnlContent = new ();

    public ErrorListForm(string title, Seq<UserError> errors)
    {
        InitializeComponents();

        lblTitle.Text = title;

        foreach (var error in errors)
            listErrors.Items.Add(error.Message);

        ThemeManager.ApplyTheme(this);
        ThemeManager.StylePrimaryButton(btnDismiss);
    }

    /// <summary>
    /// Initializes form components
    /// </summary>
    private void InitializeComponents()
    {
        Text = "Errors";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = ThemeManager.FormBackgroundColor;

        Size = new System.Drawing.Size(1500, 800);

        lblTitle = new Label
        {
            Text = "Errors",
            Dock = DockStyle.Top,
            Height = 50,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(15, 0, 0, 0)
        };
        ThemeManager.StyleLabel(lblTitle, ThemeManager.HeaderFont);

        pnlContent = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15)
        };
        ThemeManager.StylePanel(pnlContent);

        listErrors = new ListBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            IntegralHeight = false,
            Font = new System.Drawing.Font(ThemeManager.DefaultFont.FontFamily,
                                         ThemeManager.DefaultFont.Size + 1,
                                         ThemeManager.DefaultFont.Style)
        };

        btnDismiss = new Button
        {
            Text = "Dismiss",
            Dock = DockStyle.Bottom,
            Height = 60,
            Margin = new Padding(0, 15, 0, 0)
        };

        btnDismiss.Click += (s, e) => Close();

        pnlContent.Controls.Add(listErrors);
        pnlContent.Controls.Add(btnDismiss);
        Controls.Add(pnlContent);
        Controls.Add(lblTitle);

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