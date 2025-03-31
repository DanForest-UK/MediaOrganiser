using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediaOrganiser;
using MediaOrganiser.Core;
using static MediaOrganiser.Core.Types;
using LanguageExt;
using static LanguageExt.Prelude;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System;
using System.Linq;

namespace MediaOrganiser.Tests
{
    /// <summary>
    /// Tests for Windows utility methods that handle image manipulation operations
    /// </summary>
    [TestClass]
    public class WindowsTests
    {
        string testFolder;
        string testImagePath;

        /// <summary>
        /// Prepares the test environment by creating a temporary folder and test image
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            testFolder = Path.Combine(Path.GetTempPath(), "MediaOrganiserTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(testFolder);

            testImagePath = Path.Combine(testFolder, "test_image.jpg");
            CreateTestImage(testImagePath, 200, 100);
        }

        /// <summary>
        /// Cleans up the test environment by removing the temporary folder
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                if (Directory.Exists(testFolder))
                    Directory.Delete(testFolder, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up test folder: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifies that an image with no rotation maintains its original dimensions
        /// </summary>
        [TestMethod]
        public void RotateImageZero()
        {
            using var bitmap = Windows.RotateImage(Rotation.None, testImagePath).Run();
            Assert.AreEqual(200, bitmap.Width);
            Assert.AreEqual(100, bitmap.Height);
        }

        /// <summary>
        /// Verifies that a 90-degree rotation correctly swaps the image dimensions
        /// </summary>
        [TestMethod]
        public void Rotate90()
        {
            using var bitmap = Windows.RotateImage(Rotation.Rotate90, testImagePath).Run();
            Assert.AreEqual(100, bitmap.Width);
            Assert.AreEqual(200, bitmap.Height);
        }

        /// <summary>
        /// Verifies that a 180-degree rotation maintains the original image dimensions
        /// </summary>
        [TestMethod]
        public void Rotate180()
        {
            using var bitmap = Windows.RotateImage(Rotation.Rotate180, testImagePath).Run();
            Assert.AreEqual(200, bitmap.Width);
            Assert.AreEqual(100, bitmap.Height);
        }

        /// <summary>
        /// Verifies that a 270-degree rotation correctly swaps the image dimensions
        /// </summary>
        [TestMethod]
        public void Rotate270()
        {
            using var bitmap = Windows.RotateImage(Rotation.Rotate270, testImagePath).Run();
            Assert.AreEqual(100, bitmap.Width);
            Assert.AreEqual(200, bitmap.Height);
        }

        /// <summary>
        /// Verifies that attempting to rotate a non-existent file throws FileNotFoundException
        /// </summary>
        [TestMethod]
        public void RotateNoFile()
        {
            var nonExistentPath = Path.Combine(testFolder, "doesnotexist.jpg");
            Assert.ThrowsException<FileNotFoundException>(() =>
                Windows.RotateImage(Rotation.None, nonExistentPath).Run());
        }

        /// <summary>
        /// Tests that rotating and saving an image works correctly and produces a file
        /// with the expected dimensions
        /// </summary>
        [TestMethod]
        public void RotateAndSave()
        {
            var mediaInfo = TestDataFactory.CreateMediaInfoFromFile(1, testImagePath);
            var outputPath = Path.Combine(testFolder, "output.jpg");

            Windows.RotateImageAndSave(mediaInfo with { Rotation = Rotation.Rotate90 }, outputPath).Run();

            Assert.IsTrue(File.Exists(outputPath));

            using var image = Image.FromFile(outputPath);
            Assert.AreEqual(100, image.Width);
            Assert.AreEqual(200, image.Height);
        }

        /// <summary>
        /// Creates a test image with specified dimensions and a simple pattern
        /// </summary>
        void CreateTestImage(string path, int width, int height)
        {
            using var bitmap = new Bitmap(width, height);
            using var g = Graphics.FromImage(bitmap);

            g.Clear(Color.Blue);
            g.DrawRectangle(Pens.White, 0, 0, width - 1, height - 1);
            bitmap.Save(path, ImageFormat.Jpeg);
        }
    }
}