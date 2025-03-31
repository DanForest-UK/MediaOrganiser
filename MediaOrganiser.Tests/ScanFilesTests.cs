using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediaOrganiser.Logic;
using MediaOrganiser.Core;
using static MediaOrganiser.Core.Types;
using LanguageExt;
using static LanguageExt.Prelude;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Runtime.InteropServices;
using static MediaOrganiser.Core.AppErrors;

namespace MediaOrganiser.Tests.Logic
{
    /// <summary>
    /// Contains unit tests for the ScanFiles class.
    /// </summary>
    [TestClass]
    public class ScanFilesTests
    {
        private string testFolder = "";
        private string nestedFolder = "";
        private string readOnlyFolder = "";

        /// <summary>
        /// Sets up the test environment by creating folders and sample files.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Create a temporary test folder
            testFolder = Path.Combine(Path.GetTempPath(), "MediaOrganiserTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(testFolder);

            // Create nested folder for additional tests
            nestedFolder = Path.Combine(testFolder, "NestedFolder");
            Directory.CreateDirectory(nestedFolder);

            // Create read-only folder for permission tests
            readOnlyFolder = Path.Combine(testFolder, "ReadOnlyFolder");
            Directory.CreateDirectory(readOnlyFolder);

            // Create test files of various types in both folders
            // Image files
            CreateTestFile(Path.Combine(testFolder, "image1.jpg"), 1024);
            CreateTestFile(Path.Combine(testFolder, "image2.png"), 2048);
            CreateTestFile(Path.Combine(testFolder, "image3.gif"), 512);
            CreateTestFile(Path.Combine(testFolder, "image4.webp"), 1536);
            CreateTestFile(Path.Combine(nestedFolder, "nested_image1.jpg"), 768);
            CreateTestFile(Path.Combine(nestedFolder, "nested_image2.tiff"), 2560);

            // Video files
            CreateTestFile(Path.Combine(testFolder, "video1.mp4"), 4096);
            CreateTestFile(Path.Combine(testFolder, "video2.avi"), 8192);
            CreateTestFile(Path.Combine(nestedFolder, "nested_video1.mkv"), 16384);

            // Document files
            CreateTestFile(Path.Combine(testFolder, "document1.pdf"), 3072);
            CreateTestFile(Path.Combine(testFolder, "document2.docx"), 2304);
            CreateTestFile(Path.Combine(testFolder, "document3.txt"), 128);
            CreateTestFile(Path.Combine(nestedFolder, "nested_document1.rtf"), 512);

            // Files with unknown extensions
            CreateTestFile(Path.Combine(testFolder, "unknown.dat"), 256);
            CreateTestFile(Path.Combine(testFolder, "unknown.xyz"), 128);
            CreateTestFile(Path.Combine(nestedFolder, "nested_unknown.bin"), 1024);

            // Add one file to read-only folder
            CreateTestFile(Path.Combine(readOnlyFolder, "readonly_image.jpg"), 1024);

            // Try to make the read-only folder actually read-only on Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    File.SetAttributes(readOnlyFolder, FileAttributes.ReadOnly);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Could not set read-only attribute: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Cleans up the test environment after each test.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test folder after tests
            if (Directory.Exists(testFolder))
            {
                // Reset any read-only attributes
                if (Directory.Exists(readOnlyFolder))
                {
                    try
                    {
                        File.SetAttributes(readOnlyFolder, FileAttributes.Normal);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Could not reset read-only attribute: {ex.Message}");
                    }
                }

                // Delete the test directory
                try
                {
                    Directory.Delete(testFolder, true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error cleaning up test folder: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Tests the ScanFiles method to ensure it correctly scans a directory and returns the expected files.
        /// </summary>
        [TestMethod]
        public void ScanFiles()
        {
            try
            {
                var response = MediaOrganiser.Logic.ScanFiles.DoScanFiles(testFolder).Run();

                // Assert
                // Should find all media files (14 total) but not the unknown extension files
                Assert.AreEqual(14, response.Files.Count);

                // Verify file categories
                Assert.AreEqual(7, response.Files.Count(f => f.Category == FileCategory.Image));
                Assert.AreEqual(4, response.Files.Count(f => f.Category == FileCategory.Document));
                Assert.AreEqual(3, response.Files.Count(f => f.Category == FileCategory.Video));
                Assert.AreEqual(0, response.Files.Count(f => f.Category == FileCategory.Unknown));

                // Verify file extensions
                Assert.AreEqual(3, response.Files.Count(f => f.Extension.Value == ".jpg"));
                Assert.AreEqual(1, response.Files.Count(f => f.Extension.Value == ".png"));
                Assert.AreEqual(1, response.Files.Count(f => f.Extension.Value == ".gif"));
                Assert.AreEqual(1, response.Files.Count(f => f.Extension.Value == ".webp"));
                Assert.AreEqual(1, response.Files.Count(f => f.Extension.Value == ".tiff"));
                Assert.AreEqual(1, response.Files.Count(f => f.Extension.Value == ".mp4"));
                Assert.AreEqual(1, response.Files.Count(f => f.Extension.Value == ".avi"));
                Assert.AreEqual(1, response.Files.Count(f => f.Extension.Value == ".mkv"));
                Assert.AreEqual(1, response.Files.Count(f => f.Extension.Value == ".pdf"));
                Assert.AreEqual(1, response.Files.Count(f => f.Extension.Value == ".docx"));
                Assert.AreEqual(1, response.Files.Count(f => f.Extension.Value == ".txt"));
                Assert.AreEqual(1, response.Files.Count(f => f.Extension.Value == ".rtf"));
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected success but got error: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests the ScanFiles method to ensure it searches directories recursively.
        /// </summary>
        [TestMethod]
        public void ScanFilesRecursion()
        {
            try
            {
                var response = MediaOrganiser.Logic.ScanFiles.DoScanFiles(testFolder).Run();

                // Verify files from the nested folder are included
                var nestedFileCount = response.Files.Count(f =>
                    f.FullPath.Value.Contains("NestedFolder"));

                Assert.AreEqual(4, nestedFileCount);

                // Specifically check for some nested files
                Assert.IsTrue(response.Files.Any(f =>
                    f.FullPath.Value.EndsWith("nested_image1.jpg")));
                Assert.IsTrue(response.Files.Any(f =>
                    f.FullPath.Value.EndsWith("nested_document1.rtf")));
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected success but got error: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests the ScanFiles method to ensure MediaInfo objects contain correct data.
        /// </summary>
        [TestMethod]
        public void ScanFilesMediaInfoValues()
        {
            try
            {
                var response = MediaOrganiser.Logic.ScanFiles.DoScanFiles(testFolder).Run();

                var pdfFile = response.Files.FirstOrDefault(f => f.Extension.Value == ".pdf");
                Assert.IsNotNull(pdfFile);

                // Verify the file's properties
                Assert.AreEqual("document1", pdfFile.FileName.Value);
                Assert.AreEqual(FileCategory.Document, pdfFile.Category);
                Assert.AreEqual(FileState.Undecided, pdfFile.State);
                Assert.AreEqual(3072, pdfFile.Size.Value);
                Assert.AreEqual(Path.Combine(testFolder, "document1.pdf"), pdfFile.FullPath.Value);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected success but got error: {ex.Message}");
            }
        }              

        /// <summary>
        /// Tests the ScanFiles method with a non-existent folder to ensure it gives the right error
        /// </summary>
        [TestMethod]
        public void ScanFilesNonExistantFolder()
        {
            string nonExistentFolder = Path.Combine(Path.GetTempPath(), "NonExistentFolder" + Guid.NewGuid().ToString());

            var result = MediaOrganiser.Logic.ScanFiles.DoScanFiles(nonExistentFolder).Run();
            Assert.IsTrue(result.UserErrors.Any(e => e.Message.Contains(ErrorMessages.DirectoryNotFoundPrefix)));
        }

        /// <summary>
        /// Tests the ScanFiles method with an invalid path to ensure it gives the right error
        /// </summary>
        [TestMethod]
        public void ScanFilesInvalidDirectory()
        {
            // Arrange - invalid characters in path
            var invalidPath = Path.Combine(testFolder, "Invalid<>:\"/\\|?*Path");
            var result = MediaOrganiser.Logic.ScanFiles.DoScanFiles(invalidPath).Run();

            Assert.IsTrue(result.UserErrors.Any(e => e.Message.Contains(ErrorMessages.DirectoryInvalidPrefix)));
        }
             
        /// <summary>
        /// Tests the ScanFiles method with an empty path to ensure it gives the right user error.
        /// </summary>
        [TestMethod]
        public void ScanFilesEmptyPath()
        {
            var result = MediaOrganiser.Logic.ScanFiles.DoScanFiles("").Run();
            Assert.IsTrue(result.UserErrors.Count > 0);
            Assert.IsTrue(result.UserErrors.Any(e => e.Message.Contains(ErrorMessages.PathIsEmpty)));

        }
           
        /// <summary>
        /// Tests the ScanFiles method with a path that is too long to ensure it gives a user error
        /// </summary>
        [TestMethod]
        public void ScanFilesLongPath()
        {
            // Arrange - Create a path that's too long (> 260 chars on Windows)
            string longPathPart = new string('a', 50); 
            string longPath = testFolder;

            // Build up a path that's definitely too long
            for (int i = 0; i < 20; i++)
            {
                longPath = Path.Combine(longPath, longPathPart);
            }

            var result = MediaOrganiser.Logic.ScanFiles.DoScanFiles(longPath).Run();
            Assert.IsTrue(result.UserErrors.Any(er => er.Message.Contains(ErrorMessages.DirectoryNotFoundPrefix)));
        }
                  
        /// <summary>
        /// Helper method to create test files with specified size.
        /// </summary>
        /// <param name="path">The path of the test file.</param>
        /// <param name="sizeInBytes">The size of the test file in bytes.</param>
        private void CreateTestFile(string path, int sizeInBytes)
        {
            // Ensure the directory exists
            var directoryName = Path.GetDirectoryName(path);
            if (directoryName != null)
            {
                Directory.CreateDirectory(directoryName);
            }

            using (var fs = File.Create(path))
            {
                fs.SetLength(sizeInBytes);
            }

            // Set the last write time to make it more realistic
            File.SetLastWriteTime(path, DateTime.Now.AddDays(-new Random().Next(1, 30)));
        }
    }
}
