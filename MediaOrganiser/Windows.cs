using LanguageExt;
using MediaOrganiser.Domain;
using static LanguageExt.Prelude;

namespace MediaOrganiser
{
    public static class Windows
    {
        /// <summary>
        /// Load the image, apply rotation and save to destination
        /// </summary>
        /// <param name="file"></param>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        public static IO<Unit> RotateImageAndSave(MediaInfo file, string targetPath) =>
            from image in RotateImage(file.Rotation, file.FullPath.Value)
            from _ in IO.lift(() => image.Save(targetPath))
            select unit;
        
       /// <summary>
       /// Create rotated bitmap
       /// </summary>
       /// <param name="rotation"></param>
       /// <param name="path"></param>
       /// <returns></returns>
        public static IO<Bitmap> RotateImage(Rotation rotation, string path) =>
            IO.lift(() =>
            {
                using var image = System.Drawing.Image.FromFile(path);
                RotateFlipType rotateFlip = rotation switch
                {
                    Rotation.Rotate90 => RotateFlipType.Rotate90FlipNone,
                    Rotation.Rotate180 => RotateFlipType.Rotate180FlipNone,
                    Rotation.Rotate270 => RotateFlipType.Rotate270FlipNone,
                    _ => RotateFlipType.RotateNoneFlipNone
                };

                // Create a new bitmap with the rotation applied
                System.Drawing.Bitmap rotatedImage = new System.Drawing.Bitmap(image);
                rotatedImage.RotateFlip(rotateFlip);
                return rotatedImage;
            });
    }
}
