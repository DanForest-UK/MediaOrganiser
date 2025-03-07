using System;
using System.Drawing;

namespace SortPhotos.Properties
{
    /// <summary>
    /// A strongly-typed resource class for retrieving resources.
    /// </summary>
    internal class Resources
    {
        private static global::System.Resources.ResourceManager resourceMan;

        internal Resources()
        {
        }

        /// <summary>
        /// Returns the cached ResourceManager instance used by this class.
        /// </summary>
        internal static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MediaOrganizer.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        /// <summary>
        /// Returns a bin icon
        /// </summary>
        internal static Bitmap BinIcon
        {
            get
            {
                // This would normally be loaded from embedded resources
                // For demonstration, we'll create a simple icon
                return CreateSimpleIcon(Color.Red);
            }
        }

        /// <summary>
        /// Returns a tick icon
        /// </summary>
        internal static Bitmap TickIcon
        {
            get
            {
                // This would normally be loaded from embedded resources
                // For demonstration, we'll create a simple icon
                return CreateSimpleIcon(Color.Green);
            }
        }

        /// <summary>
        /// Returns a play icon
        /// </summary>
        internal static Bitmap PlayIcon
        {
            get
            {
                // This would normally be loaded from embedded resources
                // For demonstration, we'll create a simple icon
                return CreateSimpleIcon(Color.Blue);
            }
        }

        /// <summary>
        /// Returns an image placeholder
        /// </summary>
        internal static Bitmap ImagePlaceholder
        {
            get
            {
                // Create a simple image placeholder
                var bitmap = new Bitmap(100, 100);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.LightGray);
                    using (var brush = new SolidBrush(Color.DarkGray))
                    using (var font = new Font(FontFamily.GenericSansSerif, 10))
                    {
                        g.DrawString("Image", font, brush, 30, 40);
                    }
                }
                return bitmap;
            }
        }

        /// <summary>
        /// Returns a video placeholder
        /// </summary>
        internal static Bitmap VideoPlaceholder
        {
            get
            {
                // Create a simple video placeholder
                var bitmap = new Bitmap(100, 100);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.LightGray);
                    using (var brush = new SolidBrush(Color.DarkGray))
                    using (var font = new Font(FontFamily.GenericSansSerif, 10))
                    {
                        g.DrawString("Video", font, brush, 30, 40);
                    }
                }
                return bitmap;
            }
        }

        /// <summary>
        /// Creates a simple colored icon for demonstration
        /// </summary>
        private static Bitmap CreateSimpleIcon(Color color)
        {
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                using (var brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, 2, 2, 12, 12);
                }
            }
            return bitmap;
        }
    }
}