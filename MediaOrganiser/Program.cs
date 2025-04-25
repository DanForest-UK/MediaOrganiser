using MediaOrganiser.Logic;
using MediaOrganiser.WindowsSpecific;
using System.IO;

namespace MediaOrganiser;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Apply custom styles
        ApplyStyles();

        // Setup dependency for rotating image in a windows app
        Runtime.RotateImageAndSave = Images.RotateImageAndSave;

        StateSerialiser.StateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appstate.json");

        Application.Run(new MainForm());
    }

    /// <summary>
    /// Applies modern styles to the application
    /// </summary>
    private static void ApplyStyles()
    {
        ToolStripManager.Renderer = new ToolStripProfessionalRenderer();

        // Set the application's default font
        Application.SetDefaultFont(ThemeManager.DefaultFont);
    }
}
