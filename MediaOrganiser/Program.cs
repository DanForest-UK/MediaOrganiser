using LanguageExt;
using MediaOrganiser.Logic;
using static MediaOrganiser.Core.Types;

namespace MediaOrganiser
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Set up the application-wide styles
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Apply modern look and feel
            ApplyModernStyles();

            // Setup dependency for rotating image in a windows app
            Runtime.RotateImageAndSave = Windows.RotateImageAndSave;

            Application.Run(new Form1());
        }

        /// <summary>
        /// Applies modern styles to the application
        /// </summary>
        private static void ApplyModernStyles()
        {
            // Set modern renderer for menus and toolbars
            ToolStripManager.Renderer = new ToolStripProfessionalRenderer();

            // Set the application's default font
            Application.SetDefaultFont(ThemeManager.DefaultFont);
        }       
    }
}