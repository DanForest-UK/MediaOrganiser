using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Windows.Forms;

namespace MediaOrganiser.Tests;

/// <summary>
/// Tests for the ThemeManager class which provides styling and theming for the application
/// </summary>
[TestClass]
public class ThemeManagerTests
{
    /// <summary>
    /// Tests that the StyleButton method correctly applies the expected styles to a button
    /// </summary>
    [TestMethod]
    public void StyleButton()
    {
        var button = new Button();

        ThemeManager.StyleButton(button);

        Assert.AreEqual(ThemeManager.DefaultFont, button.Font);
        Assert.AreEqual(ThemeManager.ButtonBackColor, button.BackColor);
        Assert.AreEqual(ThemeManager.ButtonTextColor, button.ForeColor);
        Assert.AreEqual(FlatStyle.Flat, button.FlatStyle);
        Assert.AreEqual(ThemeManager.AccentBackColor, button.FlatAppearance.BorderColor);
        Assert.AreEqual(1, button.FlatAppearance.BorderSize);
    }

    /// <summary>
    /// Tests that the StyleLabel method correctly applies the expected styles to a label
    /// </summary>
    [TestMethod]
    public void StyleLabel()
    {
        var label = new Label();

        ThemeManager.StyleLabel(label);

        Assert.AreEqual(ThemeManager.DefaultFont, label.Font);
        Assert.AreEqual(ThemeManager.PrimaryTextColor, label.ForeColor);

        // Test with custom font
        var customFont = new Font("Arial", 12, FontStyle.Bold);
        ThemeManager.StyleLabel(label, customFont);
        Assert.AreEqual(customFont, label.Font);
    }

    /// <summary>
    /// Tests that the StyleSuccessButton method correctly applies success styling to a button
    /// </summary>
    [TestMethod]
    public void StyleSuccessButton()
    {
        var button = new Button();

        ThemeManager.StyleSuccessButton(button);

        Assert.AreEqual(ThemeManager.SuccessColor, button.BackColor);
        Assert.AreEqual(Color.White, button.ForeColor);
        Assert.AreEqual(FlatStyle.Flat, button.FlatStyle);
        Assert.AreEqual(0, button.FlatAppearance.BorderSize);
        Assert.AreEqual(Cursors.Hand, button.Cursor);
    }

    /// <summary>
    /// Tests that the StyleDangerButton method correctly applies danger styling to a button
    /// </summary>
    [TestMethod]
    public void StyleDangerButton()
    {
        var button = new Button();

        ThemeManager.StyleDangerButton(button);

        Assert.AreEqual(ThemeManager.DangerColor, button.BackColor);
        Assert.AreEqual(Color.White, button.ForeColor);
        Assert.AreEqual(FlatStyle.Flat, button.FlatStyle);
        Assert.AreEqual(0, button.FlatAppearance.BorderSize);
    }

    /// <summary>
    /// Tests that the StyleTextBox method correctly applies the expected styles to a text box
    /// </summary>
    [TestMethod]
    public void StyleTextBox()
    {
        var textBox = new TextBox();

        ThemeManager.StyleTextBox(textBox);

        Assert.AreEqual(ThemeManager.DefaultFont, textBox.Font);
        Assert.AreEqual(ThemeManager.PrimaryBackColor, textBox.BackColor);
        Assert.AreEqual(BorderStyle.FixedSingle, textBox.BorderStyle);
    }

    /// <summary>
    /// Tests that the StyleRichTextBox method correctly applies the expected styles to a rich text box
    /// </summary>
    [TestMethod]
    public void StyleRichTextBox()
    {
        var rtfBox = new RichTextBox();

        ThemeManager.StyleRichTextBox(rtfBox);

        Assert.AreEqual(ThemeManager.DefaultFont, rtfBox.Font);
        Assert.AreEqual(ThemeManager.PrimaryBackColor, rtfBox.BackColor);
        Assert.AreEqual(BorderStyle.FixedSingle, rtfBox.BorderStyle);
    }

    /// <summary>
    /// Tests that the StylePanel method correctly applies the expected styles to a panel
    /// </summary>
    [TestMethod]
    public void StylePanel()
    {
        var panel = new Panel();

        ThemeManager.StylePanel(panel);

        Assert.AreEqual(ThemeManager.SecondaryBackColor, panel.BackColor);

        // Test with custom color
        var customColor = Color.Red;
        ThemeManager.StylePanel(panel, customColor);
        Assert.AreEqual(customColor, panel.BackColor);
    }

    /// <summary>
    /// Tests that the StylePrimaryButton method correctly applies primary styling to a button
    /// </summary>
    [TestMethod]
    public void StylePrimaryButton()
    {
        var button = new Button();

        ThemeManager.StylePrimaryButton(button);

        Assert.AreEqual(ThemeManager.AccentColor, button.BackColor);
        Assert.AreEqual(Color.White, button.ForeColor);
        Assert.AreEqual(FlatStyle.Flat, button.FlatStyle);
        Assert.AreEqual(0, button.FlatAppearance.BorderSize);
    }

    /// <summary>
    /// Tests that the StyleSecondaryButton method correctly applies secondary styling to a button
    /// </summary>
    [TestMethod]
    public void StyleSecondaryButton()
    {
        var button = new Button();

        ThemeManager.StyleSecondaryButton(button);

        Assert.AreEqual(ThemeManager.SecondaryColor, button.BackColor);
        Assert.AreEqual(Color.White, button.ForeColor);
        Assert.AreEqual(FlatStyle.Flat, button.FlatStyle);
        Assert.AreEqual(0, button.FlatAppearance.BorderSize);
    }

    /// <summary>
    /// Tests that the ApplyTheme method correctly applies theme to a form
    /// </summary>
    [TestMethod]
    public void ApplyThemeToForm()
    {
        using var form = new Form();

        ThemeManager.ApplyTheme(form);

        Assert.AreEqual(ThemeManager.DefaultFont, form.Font);
        Assert.AreEqual(ThemeManager.FormBackgroundColor, form.BackColor);
    }

    /// <summary>
    /// Tests that the ApplyTheme method correctly applies theme to a user control
    /// </summary>
    [TestMethod]
    public void ApplyThemeToUserControl()
    {
        using var userControl = new UserControl();

        ThemeManager.ApplyTheme(userControl);

        Assert.AreEqual(ThemeManager.DefaultFont, userControl.Font);
        Assert.AreEqual(ThemeManager.ControlBackColor, userControl.BackColor);
    }
}