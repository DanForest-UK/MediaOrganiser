using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;
using static MediaOrganiser.Core.Types;

namespace MediaOrganiser
{
    public static class Windows
    {
        public static IO<Unit> RotateImage(MediaInfo file, string targetPath) =>
            IO.lift(() =>
            {
                // Load the image, apply rotation and save to destination
                using (var image = System.Drawing.Image.FromFile(file.FullPath.Value))
                {
                    // Apply rotation
                    RotateFlipType rotateFlip = file.Rotation switch
                    {
                        Rotation.Rotate90 => RotateFlipType.Rotate90FlipNone,
                        Rotation.Rotate180 => RotateFlipType.Rotate180FlipNone,
                        Rotation.Rotate270 => RotateFlipType.Rotate270FlipNone,
                        _ => RotateFlipType.RotateNoneFlipNone
                    };

                    // Create a new bitmap with rotation applied
                    using (var rotatedImage = new System.Drawing.Bitmap(image))
                    {
                        rotatedImage.RotateFlip(rotateFlip);
                        rotatedImage.Save(targetPath);
                    }
                }
                return unit;
            });
    }
}
