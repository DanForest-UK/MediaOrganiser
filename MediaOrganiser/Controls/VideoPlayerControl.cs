using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace MediaOrganiser
{
    /// <summary>
    /// A modern video player control using WPF MediaElement
    /// </summary>
    public class VideoPlayerControl : UserControl
    {
        // The ElementHost control hosts the WPF MediaElement
        ElementHost elementHost;

        // The WPF MediaElement
        System.Windows.Controls.MediaElement mediaElement;

        // Constructor
        public VideoPlayerControl()
        {
            InitializeControls();
        }

        /// <summary>
        /// Initializes the MediaElement and hosts it in an ElementHost
        /// </summary>
        void InitializeControls()
        {
            // Create the ElementHost that will host our WPF control
            elementHost = new ElementHost
            {
                Dock = DockStyle.Fill
            };

            // Create the MediaElement
            mediaElement = new System.Windows.Controls.MediaElement
            {
                LoadedBehavior = System.Windows.Controls.MediaState.Manual,
                UnloadedBehavior = System.Windows.Controls.MediaState.Manual,
                Stretch = Stretch.Uniform,
                Volume = 1.0
            };

            // Set the MediaElement as the ElementHost's child
            elementHost.Child = mediaElement;

            // Add the ElementHost to our control
            this.Controls.Add(elementHost);

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
        public void SetSource(string filePath)
        {
            try
            {
                // Set the source of the MediaElement
                mediaElement.Source = new Uri(filePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting media source: {ex.Message}");
            }
        }

        /// <summary>
        /// Plays the current media
        /// </summary>
        public void Play()
        {
            try
            {
                mediaElement.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing media: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops the current media
        /// </summary>
        public void Stop()
        {
            try
            {
                mediaElement.Stop();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping media: {ex.Message}");
            }
        }

        /// <summary>
        /// Pauses the current media
        /// </summary>
        public void Pause()
        {
            try
            {
                mediaElement.Pause();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error pausing media: {ex.Message}");
            }
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
                    // Stop any playing media
                    Stop();

                    // Null the MediaElement source
                    mediaElement.Source = null;

                    // Dispose the ElementHost
                    elementHost?.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing video player: {ex.Message}");
                }
            }

            base.Dispose(disposing);
        }
    }
}