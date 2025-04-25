using LanguageExt;
using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using static LanguageExt.Prelude;
using C = System.Windows.Controls;

namespace MediaOrganiser.Controls
{
    /// <summary>
    /// A modern video player control using WPF MediaElement
    /// </summary>
    public class VideoPlayerControl : UserControl
    {
        /// <summary>
        /// The ElementHost control hosts the WPF MediaElement
        /// </summary>
        ElementHost elementHost = new ();

        /// <summary>
        /// The WPF MediaElement
        /// </summary>
        C.MediaElement mediaElement = new ();

        /// <summary>
        /// Constructor
        /// </summary>
        public VideoPlayerControl()
        {
            InitializeControls();
        }

        /// <summary>
        /// Initializes the MediaElement and hosts it in an ElementHost
        /// </summary>
        void InitializeControls()
        {
            // ElementHost will host our WPF control
            elementHost = new ElementHost
            {
                Dock = DockStyle.Fill
            };

            mediaElement = new System.Windows.Controls.MediaElement
            {
                LoadedBehavior = System.Windows.Controls.MediaState.Manual,
                UnloadedBehavior = System.Windows.Controls.MediaState.Manual,
                Stretch = Stretch.Uniform,
                Volume = 1.0
            };

            // Media element needs to be element host child
            elementHost.Child = mediaElement;

            // Add the ElementHost to our control
            this.Controls.Add(elementHost);

            // Apply theme to the control
            this.BackColor = ThemeManager.PrimaryBackColor;

            // Subscribe to the MediaOpened and MediaEnded events
            mediaElement.MediaOpened += (s, e) => {
                // Auto-play when media is loaded
                mediaElement.Play();
            };

            mediaElement.MediaEnded += (s, e) => {
                // Just stop at the end
                mediaElement.Stop();
            };
        }

        /// <summary>
        /// Sets the source URI of the media to play
        /// </summary>
        public Unit SetSource(string filePath) =>
            Try.lift(() =>
            {
                // Set the source of the MediaElement
                mediaElement.Source = new Uri(filePath);
                return unit;
            }).IfFail(ex =>
            {
                System.Diagnostics.Debug.WriteLine($"Error setting media source: {ex.Message}");
                return unit;
            });

        /// <summary>
        /// Plays the current media
        /// </summary>
        public Unit Play() =>
            Try.lift(() =>
            {
                mediaElement.Play();
                return unit;
            }).IfFail(ex =>
            {
                System.Diagnostics.Debug.WriteLine($"Error playing media: {ex.Message}");
                return unit;
            });

        /// <summary>
        /// Stops the current media
        /// </summary>
        public Unit Stop() =>
            Try.lift(() =>
            {
                mediaElement.Stop();
                return unit;
            }).IfFail(ex =>
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping media: {ex.Message}");
                return unit;
            });

        /// <summary>
        /// Pauses the current media
        /// </summary>
        public Unit Pause() =>
            Try.lift(() =>
            {
                mediaElement.Pause();
                return unit;
            }).IfFail(ex =>
            {
                System.Diagnostics.Debug.WriteLine($"Error pausing media: {ex.Message}");
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
                    // Stop any playing media
                    Stop();

                    // Null the MediaElement source
                    mediaElement.Source = null;

                    // Dispose the ElementHost
                    elementHost?.Dispose();
                    return unit;
                }).IfFail(ex =>
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing video player: {ex.Message}");
                    return unit;
                });
            }

            base.Dispose(disposing);
        }
    }
}