using LanguageExt;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static LanguageExt.Prelude;
using System.Collections.Generic;
using System.Linq;
using MediaOrganiser.Domain;

namespace MediaOrganiser.Tests.Domain;

/// <summary>
/// Contains unit tests for the AppModel class.
/// </summary>
[TestClass]
public class AppModelTests
{
    /// <summary>
    /// Default AppModel instance
    /// </summary>
    static AppModel CreateDefaultModel() =>
        new(
            Files: [],
            WorkInProgress: false,
            CurrentFolder: Option<FolderPath>.None,
            CurrentFile: Option<FileId>.None,
            CopyOnly: new CopyOnly(false),
            SortByYear: new SortByYear(false),
            KeepParentFolder: new KeepParentFolder(false));

    /// <summary>
    /// Creates a test MediaInfo instance with specified properties.
    /// </summary>
    static MediaInfo CreateMediaInfo(int id, string fileName, FileCategory category, FileState state = FileState.Undecided) =>
        new(
            Id: new FileId(id),
            FileName: new FileName(fileName),
            FullPath: new FullPath($"C:/test/{fileName}"),
            Extension: new Extension(".jpg"),
            Size: new Size(1024),
            Date: new Date(System.DateTime.Now),
            Category: category,
            State: state,
            Rotation: Rotation.None);

  
    /// <summary>
    /// Tests that counting files for deletion returns the correct count with mixed file states.
    /// </summary>
    [TestMethod]
    public void FilesForDeletion()
    {
        var files = new Map<FileId, MediaInfo>()
            .Add(new FileId(1), CreateMediaInfo(1, "file1", FileCategory.Image, FileState.Keep))
            .Add(new FileId(2), CreateMediaInfo(2, "file2", FileCategory.Image, FileState.Bin))
            .Add(new FileId(3), CreateMediaInfo(3, "file3", FileCategory.Image, FileState.Undecided))
            .Add(new FileId(4), CreateMediaInfo(4, "file4", FileCategory.Image, FileState.Bin));

        var model = CreateDefaultModel() with { Files = files };
        var count = model.CountFilesForDeletion();
        Assert.AreEqual(2, count);
    }
       
    /// <summary>
    /// Tests that setting files with mixed categories filters out unknown file types.
    /// </summary>
    [TestMethod]
    public void SetFilesFilterUnknown()
    {
        var model = CreateDefaultModel();
        var files = toSeq(new List<MediaInfo>
        {
            CreateMediaInfo(1, "file1", FileCategory.Image),
            CreateMediaInfo(2, "file2", FileCategory.Unknown),
            CreateMediaInfo(3, "file3", FileCategory.Video)
        });

        var newModel = model.SetFiles(files);

        Assert.AreEqual(2, newModel.Files.Count);
        Assert.IsFalse(newModel.Files.Values.Any(f => f.Category == FileCategory.Unknown));
    }

    /// <summary>
    /// Tests that setting files sets the current file to the first file.
    /// </summary>
    [TestMethod]
    public void SetFilesFirstImage()
    {
        var model = CreateDefaultModel();
        var files = toSeq(new List<MediaInfo>
        {
            CreateMediaInfo(1, "doc1", FileCategory.Document),
            CreateMediaInfo(2, "vid1", FileCategory.Video)
        });

        var newModel = model.SetFiles(files);

        Assert.IsTrue(newModel.CurrentFile.IsSome);
        Assert.AreEqual(1, newModel.CurrentFile.Map(i => i.Value).IfNone(0));
    }       

    /// <summary>
    /// Tests that rotating an image by 90 degrees updates the rotation correctly.
    /// </summary>
    [TestMethod]
    public void Rotate90()
    {
        var fileId = new FileId(1);
        var file = CreateMediaInfo(1, "image", FileCategory.Image);
        var files = new Map<FileId, MediaInfo>().Add(fileId, file);
        var model = CreateDefaultModel() with
        {
            Files = files,
            CurrentFile = Option<FileId>.Some(fileId)
        };

        var newModel = model.RotateCurrentImage(Rotation.Rotate90);

        Assert.AreEqual(Rotation.Rotate90, newModel.Files[fileId].Rotation);
    }

    /// <summary>
    /// Tests that rotating an image by 270 degrees updates the rotation correctly.
    /// </summary>
    [TestMethod]
    public void Rotate720()
    {
        var fileId = new FileId(1);
        var file = CreateMediaInfo(1, "image", FileCategory.Image);
        var files = new Map<FileId, MediaInfo>().Add(fileId, file);
        var model = CreateDefaultModel() with
        {
            Files = files,
            CurrentFile = Option<FileId>.Some(fileId)
        };

        var newModel = model.RotateCurrentImage(Rotation.Rotate270);

        Assert.AreEqual(Rotation.Rotate270, newModel.Files[fileId].Rotation);
    }

    /// <summary>
    /// Tests that multiple rotations accumulate correctly.
    /// </summary>
    [TestMethod]
    public void RotateAccumulates()
    {
        var fileId = new FileId(1);
        var file = CreateMediaInfo(1, "image", FileCategory.Image);
        var files = new Map<FileId, MediaInfo>().Add(fileId, file);
        var model = CreateDefaultModel() with
        {
            Files = files,
            CurrentFile = Option<FileId>.Some(fileId)
        };

        var intermediateModel = model.RotateCurrentImage(Rotation.Rotate90);
        var finalModel = intermediateModel.RotateCurrentImage(Rotation.Rotate90);

        Assert.AreEqual(Rotation.Rotate180, finalModel.Files[fileId].Rotation);
    }

    /// <summary>
    /// Tests that a full circle of rotations returns to the original rotation.
    /// </summary>
    [TestMethod]
    public void FullRotation()
    {
        var fileId = new FileId(1);
        var file = CreateMediaInfo(1, "image", FileCategory.Image);
        var files = new Map<FileId, MediaInfo>().Add(fileId, file);
        var model = CreateDefaultModel() with
        {
            Files = files,
            CurrentFile = Option<FileId>.Some(fileId)
        };

        var model1 = model.RotateCurrentImage(Rotation.Rotate90);
        var model2 = model1.RotateCurrentImage(Rotation.Rotate90);
        var model3 = model2.RotateCurrentImage(Rotation.Rotate90);
        var model4 = model3.RotateCurrentImage(Rotation.Rotate90);

        Assert.AreEqual(Rotation.None, model4.Files[fileId].Rotation);
    }
      
    /// <summary>
    /// Tests that moving to the next file when at the last file returns the unchanged model.
    /// </summary>
    [TestMethod]
    public void NextFileOnLastFile()
    {
        var files = new Map<FileId, MediaInfo>()
            .Add(new FileId(1), CreateMediaInfo(1, "file1", FileCategory.Image))
            .Add(new FileId(2), CreateMediaInfo(2, "file2", FileCategory.Image));

        var model = CreateDefaultModel() with
        {
            Files = files,
            CurrentFile = Option<FileId>.Some(new FileId(2))
        };

        var newModel = model.MoveToNextFile();

        Assert.AreEqual(model, newModel);
    }

    /// <summary>
    /// Tests that moving to the next file with more files available updates the current file correctly.
    /// </summary>
    [TestMethod]
    public void NextFile()
    {
        var files = new Map<FileId, MediaInfo>()
            .Add(new FileId(1), CreateMediaInfo(1, "file1", FileCategory.Image))
            .Add(new FileId(2), CreateMediaInfo(2, "file2", FileCategory.Image))
            .Add(new FileId(3), CreateMediaInfo(3, "file3", FileCategory.Image));

        var model = CreateDefaultModel() with
        {
            Files = files,
            CurrentFile = Option<FileId>.Some(new FileId(1))
        };

        var newModel = model.MoveToNextFile();

        Assert.IsTrue(newModel.CurrentFile.IsSome);
        Assert.AreEqual(2, newModel.CurrentFile.Map(id => id.Value).IfNone(0));
    }
       
    /// <summary>
    /// Tests that moving to the previous file when at the first file returns the unchanged model.
    /// </summary>
    [TestMethod]
    public void PreviousFileFirstFile()
    {
        var files = new Map<FileId, MediaInfo>()
            .Add(new FileId(1), CreateMediaInfo(1, "file1", FileCategory.Image))
            .Add(new FileId(2), CreateMediaInfo(2, "file2", FileCategory.Image));

        var model = CreateDefaultModel() with
        {
            Files = files,
            CurrentFile = Option<FileId>.Some(new FileId(1))
        };

        var newModel = model.MoveToPreviousFile();

        Assert.AreEqual(model, newModel);
    }

    /// <summary>
    /// Tests that moving to the previous file with available previous files updates the current file correctly.
    /// </summary>
    [TestMethod]
    public void PreviousFile()
    {
        var files = new Map<FileId, MediaInfo>()
            .Add(new FileId(1), CreateMediaInfo(1, "file1", FileCategory.Image))
            .Add(new FileId(2), CreateMediaInfo(2, "file2", FileCategory.Image))
            .Add(new FileId(3), CreateMediaInfo(3, "file3", FileCategory.Image));

        var model = CreateDefaultModel() with
        {
            Files = files,
            CurrentFile = Option<FileId>.Some(new FileId(3))
        };

        var newModel = model.MoveToPreviousFile();

        Assert.IsTrue(newModel.CurrentFile.IsSome);
        Assert.AreEqual(2, newModel.CurrentFile.Map(id => id.Value).IfNone(0));
    }

    /// <summary>
    /// Tests that updating file state with an existing file updates the state correctly.
    /// </summary>
    [TestMethod]
    public void UpdateState()
    {
        var fileId = new FileId(1);
        var file = CreateMediaInfo(1, "file1", FileCategory.Image);
        var files = new Map<FileId, MediaInfo>().Add(fileId, file);

        var model = CreateDefaultModel() with
        {
            Files = files,
            CurrentFile = Option<FileId>.Some(fileId)
        };

        var newModel = model.UpdateFileState(FileState.Keep);

        Assert.AreEqual(FileState.Keep, newModel.Files[fileId].State);
    }

    /// <summary>
    /// Tests that updating filename with an existing file updates the filename correctly.
    /// </summary>
    [TestMethod]
    public void UpdateFileName()
    {
        var fileId = new FileId(1);
        var file = CreateMediaInfo(1, "oldname", FileCategory.Image);
        var files = new Map<FileId, MediaInfo>().Add(fileId, file);

        var model = CreateDefaultModel() with
        {
            Files = files,
            CurrentFile = fileId
        };

        var newModel = model.UpdateFilename("newname");

        Assert.AreEqual("newname", newModel.Files[fileId].FileName.Value);
    }
}
