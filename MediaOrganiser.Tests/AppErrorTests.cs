using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediaOrganiser.Domain;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace MediaOrganiser.Tests.Domain;

/// <summary>
/// Contains unit tests for the AppErrors class.
/// </summary>
[TestClass]
public class AppErrorsTests
{
    /// <summary>
    /// Tests that DisplayError method correctly creates an error with the specified message.
    /// </summary>
    [TestMethod]
    public void DisplayErrorWithoutInner()
    {
        const string message = "Test error message";
        var error = AppErrors.DisplayError(message, None);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.AreEqual(message, error.Message);
        Assert.IsTrue(error.Inner.IsNone);
    }

    /// <summary>
    /// Tests that DisplayError method correctly includes inner error when provided.
    /// </summary>
    [TestMethod]
    public void DisplayErrorWithInner()
    {
        const string message = "Test error message";
        var innerError = Error.New("Inner error");
        var error = AppErrors.DisplayError(message, innerError);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.AreEqual(message, error.Message);
        Assert.IsTrue(error.Inner.IsSome);
        error.Inner.IfSome(inner => Assert.AreEqual(innerError.Message, inner.Message));
    }

    /// <summary>
    /// Tests that ThereWasAProblem method correctly creates an error with the expected message.
    /// </summary>
    [TestMethod]
    public void ThereWasAProblem()
    {
        var innerError = Error.New("Test inner error");
        var error = AppErrors.ThereWasAProblem(innerError);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.AreEqual(ErrorMessages.ThereWasAProblem, error.Message);
        Assert.IsTrue(error.Inner.IsSome);
        error.Inner.IfSome(inner => Assert.AreEqual(innerError.Message, inner.Message));
    }

    /// <summary>
    /// Tests that NeedFileSystemAccess method correctly creates an error with the expected message.
    /// </summary>
    [TestMethod]
    public void NeedFileSystemAccess()
    {
        var innerError = Error.New("Test inner error");
        var error = AppErrors.NeedFileSystemAccess(innerError);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.AreEqual(ErrorMessages.FileSystemAccessNeeded, error.Message);
        Assert.IsTrue(error.Inner.IsSome);
        error.Inner.IfSome(inner => Assert.AreEqual(innerError.Message, inner.Message));
    }

    /// <summary>
    /// Tests that AccessToPathDenied method correctly includes the path in the error message.
    /// </summary>
    [TestMethod]
    public void AccessToPathDenied()
    {
        var innerError = Error.New("Test inner error");
        var path = "C:\\test\\path";
        var error = AppErrors.AccessToPathDenied(path, innerError);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.IsTrue(error.Message.Contains(path));
        Assert.IsTrue(error.Message.Contains(ErrorMessages.AccessToPathDeniedPrefix));
        Assert.IsTrue(error.Inner.IsSome);
        error.Inner.IfSome(inner => Assert.AreEqual(innerError.Message, inner.Message));
    }

    /// <summary>
    /// Tests that PathIsEmpty method creates an error with the expected message.
    /// </summary>
    [TestMethod]
    public void PathIsEmpty()
    {
        var innerError = Error.New("Test inner error");
        var error = AppErrors.PathIsEmpty(innerError);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.AreEqual(ErrorMessages.PathIsEmpty, error.Message);
        Assert.IsTrue(error.Inner.IsSome);
        error.Inner.IfSome(inner => Assert.AreEqual(innerError.Message, inner.Message));
    }

    /// <summary>
    /// Tests that DirectoryInvalid method correctly includes the path in the error message.
    /// </summary>
    [TestMethod]
    public void DirectoryInvalid()
    {
        var innerError = Error.New("Test inner error");
        var path = "C:\\test\\invalid*path";
        var error = AppErrors.DirectoryInvalid(innerError, path);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.IsTrue(error.Message.Contains(path));
        Assert.IsTrue(error.Message.Contains(ErrorMessages.DirectoryInvalidPrefix));
        Assert.IsTrue(error.Inner.IsSome);
        error.Inner.IfSome(inner => Assert.AreEqual(innerError.Message, inner.Message));
    }

    /// <summary>
    /// Tests that GetFilesError method correctly includes the extension in the error message.
    /// </summary>
    [TestMethod]
    public void GetFilesError()
    {
        var innerError = Error.New("Test inner error");
        var extension = ".jpg";
        var error = AppErrors.GetFilesError(extension, innerError);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.IsTrue(error.Message.Contains(extension));
        Assert.IsTrue(error.Message.Contains(ErrorMessages.ErrorGettingFilesPrefix));
        Assert.IsTrue(error.Inner.IsSome);
        error.Inner.IfSome(inner => Assert.AreEqual(innerError.Message, inner.Message));
    }

    /// <summary>
    /// Tests that UnauthorisedAccess method correctly includes the location in the error message.
    /// </summary>
    [TestMethod]
    public void UnauthorisedAccess()
    {
        var innerError = Error.New("Test inner error");
        var location = "C:\\test\\secure";
        var error = AppErrors.UnauthorisedAccess(location, innerError);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.IsTrue(error.Message.Contains(location));
        Assert.IsTrue(error.Message.Contains(ErrorMessages.UnauthorisedAccessPrefix));
        Assert.IsTrue(error.Inner.IsSome);
        error.Inner.IfSome(inner => Assert.AreEqual(innerError.Message, inner.Message));
    }

    /// <summary>
    /// Tests that FileNotFound method correctly includes the path in the error message.
    /// </summary>
    [TestMethod]
    public void FileNotFound_WithoutInner()
    {
        var path = "C:\\test\\missing.jpg";
        var error = AppErrors.FileNotFound(path, None);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.IsTrue(error.Message.Contains(path));
        Assert.IsTrue(error.Message.Contains(ErrorMessages.FileNotFoundPrefix));
        Assert.IsTrue(error.Inner.IsNone);
    }

    /// <summary>
    /// Tests that FileNotFound method correctly includes inner error when provided.
    /// </summary>
    [TestMethod]
    public void FileNotFound_WithInner()
    {
        var innerError = Error.New("Test inner error");
        var path = "C:\\test\\missing.jpg";
        var error = AppErrors.FileNotFound(path, innerError);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.IsTrue(error.Message.Contains(path));
        Assert.IsTrue(error.Message.Contains(ErrorMessages.FileNotFoundPrefix));
        Assert.IsTrue(error.Inner.IsSome);
        error.Inner.IfSome(inner => Assert.AreEqual(innerError.Message, inner.Message));
    }

    /// <summary>
    /// Tests that DirectoryNotFound method correctly includes the path in the error message.
    /// </summary>
    [TestMethod]
    public void DirectoryNotFound()
    {
        var innerError = Error.New("Test inner error");
        var path = "C:\\test\\missing";
        var error = AppErrors.DirectoryNotFound(path, innerError);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.IsTrue(error.Message.Contains(path));
        Assert.IsTrue(error.Message.Contains(ErrorMessages.DirectoryNotFoundPrefix));
        Assert.IsTrue(error.Inner.IsSome);
        error.Inner.IfSome(inner => Assert.AreEqual(innerError.Message, inner.Message));
    }

    /// <summary>
    /// Tests that ReadFileError method correctly includes the filename in the error message.
    /// </summary>
    [TestMethod]
    public void ReadFileError()
    {
        var innerError = Error.New("Test inner error");
        var filename = "test.jpg";
        var error = AppErrors.ReadFileError(filename, innerError);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.IsTrue(error.Message.Contains(filename));
        Assert.IsTrue(error.Message.Contains(ErrorMessages.ErrorReadingFilePrefix));
        Assert.IsTrue(error.Inner.IsSome);
        error.Inner.IfSome(inner => Assert.AreEqual(innerError.Message, inner.Message));
    }

    /// <summary>
    /// Tests that UnableToMove method correctly includes the path in the error message.
    /// </summary>
    [TestMethod]
    public void UnableToMove()
    {
        var innerError = Error.New("Test inner error");
        var path = "C:\\test\\file.jpg";
        var error = AppErrors.UnableToMove(path, innerError);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.IsTrue(error.Message.Contains(path));
        Assert.IsTrue(error.Message.Contains(ErrorMessages.UnableToMovePrefix));
        Assert.IsTrue(error.Inner.IsSome);
        error.Inner.IfSome(inner => Assert.AreEqual(innerError.Message, inner.Message));
    }

    /// <summary>
    /// Tests that UnableToDelete method correctly includes the filename in the error message.
    /// </summary>
    [TestMethod]
    public void UnableToDelete()
    {
        var innerError = Error.New("Test inner error");
        var filename = "test.jpg";
        var error = AppErrors.UnableToDelete(filename, innerError);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.IsTrue(error.Message.Contains(filename));
        Assert.IsTrue(error.Message.Contains(ErrorMessages.UnableToDeletePrefix));
        Assert.IsTrue(error.Inner.IsSome);
        error.Inner.IfSome(inner => Assert.AreEqual(innerError.Message, inner.Message));
    }

    /// <summary>
    /// Tests that UnableToRotate method correctly includes the filename and suffix in the error message.
    /// </summary>
    [TestMethod]
    public void UnableToRotate()
    {
        var innerError = Error.New("Test inner error");
        var filename = "test.jpg";
        var error = AppErrors.UnableToRotate(filename, innerError);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.IsTrue(error.Message.Contains(filename));
        Assert.IsTrue(error.Message.Contains(ErrorMessages.UnableToRotatePrefix));
        Assert.IsTrue(error.Message.Contains(ErrorMessages.UnableToRotateSuffix));
        Assert.IsTrue(error.Inner.IsSome);
        error.Inner.IfSome(inner => Assert.AreEqual(innerError.Message, inner.Message));
    }

    /// <summary>
    /// Tests that UnableToCreateDirectory method correctly includes the directory in the error message.
    /// </summary>
    [TestMethod]
    public void UnableToCreateDirectory()
    {
        var innerError = Error.New("Test inner error");
        var directory = "C:\\test\\new";
        var error = AppErrors.UnableToCreateDirectory(directory, innerError);

        Assert.AreEqual(UserError.DisplayErrorCode, error.Code);
        Assert.IsTrue(error.Message.Contains(directory));
        Assert.IsTrue(error.Message.Contains(ErrorMessages.UnableToCreateDirectoryPrefix));
        Assert.IsTrue(error.Inner.IsSome);
        error.Inner.IfSome(inner => Assert.AreEqual(innerError.Message, inner.Message));
    }
}