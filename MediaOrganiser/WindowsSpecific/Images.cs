using LanguageExt;
using MediaOrganiser.Domain;
using static LanguageExt.Prelude;
using D = System.Drawing;

namespace MediaOrganiser.WindowsSpecific;

    /// <summary>
    /// Code that is specific to Windows implementation. 
    /// Please update assign different delegate to Logic.Runtime if you want to reuse the other architecture
    /// in a non-windows system
    /// </summary>
    public static class Images
{
    /// <summary>
    /// Load the image, apply rotation and save to destination
    /// </summary>
    public static IO<Unit> RotateImageAndSave(MediaInfo file, string targetPath) =>
     from image in RotateImage(file.FullPath.Value, file.Rotation)
     from _ in SaveImage(image, targetPath)
     select unit;

    /// <summary>
    /// Save image
    /// </summary>
    public static IO<Unit> SaveImage(Bitmap image, string path) =>
        IO.lift(() => image.Save(path));

    /// <summary>
    /// Load image
    /// </summary>
    public static IO<D.Image> ImageFromFile(string path) =>
        IO.lift(() => D.Image.FromFile(path));

    /// <summary>
    /// Create rotated bitmap
    /// </summary>
    /// <param name="rotation"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static IO<Bitmap> RotateImage(string path, Rotation rotation) =>
    from image in use(ImageFromFile(path))
    from result in RotateImage(image, rotation)
    from _ in release(image)
    select result;

    public static IO<Bitmap> RotateImage(D.Image image, Rotation rotation) =>
        IO.lift(() =>
        {
            var bitmap = new Bitmap(image);
            bitmap.RotateFlip(rotation.ToFlipType());
            return bitmap;
        });
}

