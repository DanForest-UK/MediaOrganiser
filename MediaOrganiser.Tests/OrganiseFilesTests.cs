using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediaOrganiser.Logic;
using static LanguageExt.Prelude;
using System.Diagnostics;
using MediaOrganiser.Domain;

namespace MediaOrganiser.Tests.Logic;

/// <summary>
/// Tests for the OrganiseFiles class which handles organization of media files
/// </summary>
[TestClass]
public class OrganiseFilesTests
{
    private string testSourceFolder = "";
    private string testDestinationFolder = "";
    private string testBaseFolder = "";

    /// <summary>
    /// Sets up the test environment by creating folders and sample files
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        // Create temporary test folders
        testBaseFolder = Path.Combine(Path.GetTempPath(), "MediaOrganiserTests", Guid.NewGuid().ToString());
        testSourceFolder = Path.Combine(testBaseFolder, "Source");
        testDestinationFolder = Path.Combine(testBaseFolder, "Destination");

        Directory.CreateDirectory(testSourceFolder);
        Directory.CreateDirectory(testDestinationFolder);

        // Create test files of different types
        TestDataFactory.CreateTestFile(Path.Combine(testSourceFolder, "image1.jpg"), 1024);
        TestDataFactory.CreateTestFile(Path.Combine(testSourceFolder, "image2.png"), 2048);
        TestDataFactory.CreateTestFile(Path.Combine(testSourceFolder, "document1.pdf"), 3072);
        TestDataFactory.CreateTestFile(Path.Combine(testSourceFolder, "video1.mp4"), 4096);

        // Create a subfolder with files to test parent folder preservation
        var subFolder = Path.Combine(testSourceFolder, "Subfolder");
        Directory.CreateDirectory(subFolder);
        TestDataFactory.CreateTestFile(Path.Combine(subFolder, "subimage.jpg"), 1024);

        // Create an additional subfolder to test more complex structures
        var nestedFolder = Path.Combine(subFolder, "Nested");
        Directory.CreateDirectory(nestedFolder);
        TestDataFactory.CreateTestFile(Path.Combine(nestedFolder, "nestedimage.jpg"), 2048);
    }

    /// <summary>
    /// Cleans up test resources after each test
    /// </summary>
    [TestCleanup]
    public void Cleanup()
    {
        try
        {
            if (Directory.Exists(testBaseFolder))
                Directory.Delete(testBaseFolder, true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cleaning up test folders: {ex.Message}");
        }
    }

    /// <summary>
    /// Tests that files are organized correctly based on the copy-only mode and handles Bin file state
    /// </summary>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void OrganiseFilesCopyOnly(bool copyOnly)
    {
        var imagePath = Path.Combine(testSourceFolder, "image1.jpg");
        var binImagePath = Path.Combine(testSourceFolder, "image2.png");

        var imageFile = TestDataFactory.CreateMediaInfoFromFile(1, imagePath, FileState.Keep);
        var binImageFile = TestDataFactory.CreateMediaInfoFromFile(2, binImagePath, FileState.Bin);

        var files = toSeq([imageFile, binImageFile]);
        var options = CreateOrganizationOptions(copyOnly: copyOnly);
        var result = OrganiseFiles.DoOrganiseFiles(files, testDestinationFolder, options.copyOnly, options.sortByYear, options.keepParentFolder).Run();

        Assert.IsTrue(result.IsEmpty); // No errors

        if (copyOnly)
        {
            // File should exist in original location
            Assert.IsTrue(File.Exists(imagePath));
            // Bin file should still exist in original location
            Assert.IsTrue(File.Exists(binImagePath));
            // Bin file should not be copied to destination
            Assert.IsFalse(File.Exists(Path.Combine(testDestinationFolder, "Images", "image2.jpg")));
        }
        else
        {
            // File should not exist in original location
            Assert.IsFalse(File.Exists(imagePath));
            // Bin file should be deleted from original location
            Assert.IsFalse(File.Exists(binImagePath));
            // Bin file should not be copied to destination
            Assert.IsFalse(File.Exists(Path.Combine(testDestinationFolder, "Images", "image2.jpg")));
        }

        // File should be copied/moved to destination in the Images folder
        Assert.IsTrue(File.Exists(Path.Combine(testDestinationFolder, "Images", "image1.jpg")));
    }

    /// <summary>
    /// Tests that files of different categories are organized into the correct folders
    /// </summary>
    [TestMethod]
    public void OrganiseFilesByFileType()
    {
        var imagePath = Path.Combine(testSourceFolder, "image1.jpg");
        var documentPath = Path.Combine(testSourceFolder, "document1.pdf");
        var videoPath = Path.Combine(testSourceFolder, "video1.mp4");

        var imageFile = TestDataFactory.CreateMediaInfoFromFile(1, imagePath, FileState.Keep);
        var documentFile = TestDataFactory.CreateMediaInfoFromFile(2, documentPath, FileState.Keep);
        var videoFile = TestDataFactory.CreateMediaInfoFromFile(3, videoPath, FileState.Keep);

        var files = toSeq([imageFile, documentFile, videoFile]);
        var (copyOnly, sortByYear, keepParentFolder) = CreateOrganizationOptions(copyOnly: true);

        var result = OrganiseFiles.DoOrganiseFiles(files, testDestinationFolder, copyOnly, sortByYear, keepParentFolder).Run();

        Assert.IsTrue(result.IsEmpty); // No errors

        // Each file should be copied to the appropriate category folder
        Assert.IsTrue(File.Exists(Path.Combine(testDestinationFolder, "Images", "image1.jpg")));
        Assert.IsTrue(File.Exists(Path.Combine(testDestinationFolder, "Documents", "document1.pdf")));
        Assert.IsTrue(File.Exists(Path.Combine(testDestinationFolder, "Videos", "video1.mp4")));
    }

    /// <summary>
    /// Tests that filename conflicts are resolved by creating unique filenames
    /// </summary>
    [TestMethod]
    public void OrganiseFilesUniqueNames()
    {
        // Create two files with same name
        var sourcePath = Path.Combine(testSourceFolder, "image1.jpg");

        // Same image twice - second should not overwrite first
        var imageFile1 = TestDataFactory.CreateMediaInfoFromFile(1, sourcePath, FileState.Keep);
        var imageFile2 = TestDataFactory.CreateMediaInfoFromFile(2, sourcePath, FileState.Keep);

        var files = toSeq([imageFile1, imageFile2]);
        var (copyOnly, sortByYear, keepParentFolder) = CreateOrganizationOptions(copyOnly: true);

        var result = OrganiseFiles.DoOrganiseFiles(files, testDestinationFolder, copyOnly, sortByYear, keepParentFolder).Run();

        Assert.IsTrue(result.IsEmpty); // No errors

        Assert.IsTrue(File.Exists(Path.Combine(testDestinationFolder, "Images", "image1.jpg")));

        // Second file should have a unique name (e.g., image11.jpg)
        var destinationDir = Path.Combine(testDestinationFolder, "Images");
        var destFiles = Directory.GetFiles(destinationDir);
        Assert.AreEqual(2, destFiles.Length); // There should be 2 files
        Assert.IsTrue(destFiles.Any(f => f != Path.Combine(destinationDir, "image1.jpg"))); // And one should be different from the first
    }

    /// <summary>
    /// Tests that non-existent destination folders are created automatically
    /// </summary>
    [TestMethod]
    public void OrganiseFilesNonExistantDirectory()
    {
        var nonExistentDest = Path.Combine(testDestinationFolder, "NonExistent");
        var imagePath = Path.Combine(testSourceFolder, "image1.jpg");
        var imageFile = TestDataFactory.CreateMediaInfoFromFile(1, imagePath, FileState.Keep);

        var files = toSeq([imageFile]);
        var (copyOnly, sortByYear, keepParentFolder) = CreateOrganizationOptions(copyOnly: true);

        var result = OrganiseFiles.DoOrganiseFiles(files, nonExistentDest, copyOnly, sortByYear, keepParentFolder).Run();

        Assert.IsTrue(result.IsEmpty); // No errors

        // Directories should be created
        Assert.IsTrue(Directory.Exists(nonExistentDest));
        Assert.IsTrue(Directory.Exists(Path.Combine(nonExistentDest, "Images")));

        // File should be copied
        Assert.IsTrue(File.Exists(Path.Combine(nonExistentDest, "Images", "image1.jpg")));
    }

    /// <summary>
    /// Tests that files are organized by year when sortByYear is true
    /// </summary>
    [TestMethod]
    public void OrganiseFilesSortByYear()
    {
        var currentYear = DateTime.Now.Year.ToString();
        var imagePath = Path.Combine(testSourceFolder, "image1.jpg");
        var imageFile = TestDataFactory.CreateMediaInfoFromFile(1, imagePath, FileState.Keep);

        var files = toSeq([imageFile]);
        var (copyOnly, sortByYear, keepParentFolder) = CreateOrganizationOptions(copyOnly: true, sortByYear: true);
        var result = OrganiseFiles.DoOrganiseFiles(files, testDestinationFolder, copyOnly, sortByYear, keepParentFolder).Run();

        Assert.IsTrue(result.IsEmpty); // No errors

        // File should be copied to destination in the Images/{Year} folder
        Assert.IsTrue(File.Exists(Path.Combine(testDestinationFolder, "Images", currentYear, "image1.jpg")));
    }

    /// <summary>
    /// Tests that parent folder structure is preserved when keepParentFolder is true
    /// </summary>
    [TestMethod]
    public void OragniseFilesParentDirectory()
    {
        var subfolder = "Subfolder";
        var imagePath = Path.Combine(testSourceFolder, subfolder, "subimage.jpg");
        var imageFile = TestDataFactory.CreateMediaInfoFromFile(1, imagePath, FileState.Keep);

        var files = toSeq([imageFile]);
        var (copyOnly, sortByYear, keepParentFolder) = CreateOrganizationOptions(copyOnly: true, keepParentFolder: true);

        var result = OrganiseFiles.DoOrganiseFiles(files, testDestinationFolder, copyOnly, sortByYear, keepParentFolder).Run();

        Assert.IsTrue(result.IsEmpty);

        // File should be copied to destination in the Images/Subfolder folder
        Assert.IsTrue(File.Exists(Path.Combine(testDestinationFolder, "Images", subfolder, "subimage.jpg")));
    }

    /// <summary>
    /// Tests that year sorting and parent folder preservation can be combined
    /// </summary>
    [TestMethod]
    public void OrganiseFilesByYearAndParentFolder()
    {
        var currentYear = DateTime.Now.Year.ToString();
        var subfolder = "Subfolder";
        var imagePath = Path.Combine(testSourceFolder, subfolder, "subimage.jpg");
        var imageFile = TestDataFactory.CreateMediaInfoFromFile(1, imagePath, FileState.Keep);

        var files = toSeq([imageFile]);
        var (copyOnly, sortByYear, keepParentFolder) = CreateOrganizationOptions(copyOnly: true, sortByYear: true, keepParentFolder: true);

        var result = OrganiseFiles.DoOrganiseFiles(files, testDestinationFolder, copyOnly, sortByYear, keepParentFolder).Run();

        Assert.IsTrue(result.IsEmpty); // no errors

        // File should be copied to destination with both parent folder and year
        Assert.IsTrue(File.Exists(Path.Combine(testDestinationFolder, "Images", subfolder, currentYear, "subimage.jpg")));
    }

    /// <summary>
    /// Tests that a missing source file results in an error
    /// </summary>
    [TestMethod]
    public void OrganiseFilesMissingFile()
    {
        // Create MediaInfo for file that doesn't exist
        var nonExistentPath = Path.Combine(testSourceFolder, "nonexistent.jpg");
        var nonExistentFile = TestDataFactory.CreateMediaInfo(
            1,
            "nonexistent",
            FileCategory.Image,
            nonExistentPath,
            ".jpg",
            FileState.Keep);

        var files = toSeq([nonExistentFile]);
        var (copyOnly, sortByYear, keepParentFolder) = CreateOrganizationOptions();

        var result = OrganiseFiles.DoOrganiseFiles(files, testDestinationFolder, copyOnly, sortByYear, keepParentFolder).Run();

        Assert.IsFalse(result.IsEmpty); // Should have errors
        Assert.AreEqual(1, result.Count); // Should be one error
        Assert.IsTrue(result.First().Message.Contains("File not found")); // Error should mention file not found
    }

    /// <summary>
    /// Tests that an invalid destination path results in an error
    /// </summary>
    [TestMethod]
    public void OrganiseFilesInvalidPath()
    {
        var invalidDestPath = Path.Combine(testDestinationFolder, "Invalid|Path");
        var imagePath = Path.Combine(testSourceFolder, "image1.jpg");
        var imageFile = TestDataFactory.CreateMediaInfoFromFile(1, imagePath, FileState.Keep);

        var files = toSeq([imageFile]);
        var (copyOnly, sortByYear, keepParentFolder) = CreateOrganizationOptions();
        var result = OrganiseFiles.DoOrganiseFiles(files, invalidDestPath, copyOnly, sortByYear, keepParentFolder).Run();

        Assert.IsFalse(result.IsEmpty); // Should have errors
        // The error should be related to the invalid path
        Assert.IsTrue(result.First().Message.Contains("Unable to create directory"));
    }

    /// <summary>
    /// Tests that file operation errors are collected and returned
    /// </summary>
    [TestMethod]
    public void OrganiseFilePermissionError()
    {
        // Create a read-only file to simulate a permission error
        var readOnlyFilePath = Path.Combine(testSourceFolder, "readonly.jpg");
        TestDataFactory.CreateTestFile(readOnlyFilePath, 1024);
        File.SetAttributes(readOnlyFilePath, FileAttributes.ReadOnly);

        var readOnlyFile = TestDataFactory.CreateMediaInfoFromFile(1, readOnlyFilePath, FileState.Bin);

        var missingFilePath = Path.Combine(testSourceFolder, "missing.jpg");
        var missingFile = TestDataFactory.CreateMediaInfo(
            2,
            "missing",
            FileCategory.Image,
            missingFilePath,
            ".jpg",
            FileState.Keep);

        var files = toSeq([readOnlyFile, missingFile]);
        var (copyOnly, sortByYear, keepParentFolder) = CreateOrganizationOptions(copyOnly: false); // Try to delete a read-only file

        var result = OrganiseFiles.DoOrganiseFiles(files, testDestinationFolder, copyOnly, sortByYear, keepParentFolder).Run();

        Assert.IsFalse(result.IsEmpty); // Should have errors
        Assert.AreEqual(2, result.Count); // Should have 2 errors (one for each file)

        // Clean up the read-only flag
        File.SetAttributes(readOnlyFilePath, FileAttributes.Normal);
    }

    /// <summary>
    /// Helper method to create organization options with specified settings
    /// </summary>
    static (CopyOnly copyOnly, SortByYear sortByYear, KeepParentFolder keepParentFolder) CreateOrganizationOptions(
        bool copyOnly = true,
        bool sortByYear = false,
        bool keepParentFolder = false) =>
        (
            new CopyOnly(copyOnly),
            new SortByYear(sortByYear),
            new KeepParentFolder(keepParentFolder)
        );
}