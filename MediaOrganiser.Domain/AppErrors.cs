using LanguageExt;
using LanguageExt.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MediaOrganiser.Domain;

/// <summary>
/// Provides factory methods for creating standardized error objects throughout the application.
/// </summary>
public static class AppErrors
{
    /// <summary>
    /// Creates a generic display error with an optional inner error.
    /// </summary>
    public static Error DisplayError(string message, Option<Error> inner) =>
        inner.Match(
            Some: inn => Error.New(UserError.DisplayErrorCode, message, inn),
            None: () => Error.New(UserError.DisplayErrorCode, message));

    /// <summary>
    /// Creates a generic "there was a problem" error.
    /// </summary>
    public static Error ThereWasAProblem(Error inner) =>
        DisplayError(ErrorMessages.ThereWasAProblem, inner);

    /// <summary>
    /// Creates an error indicating that file system access is required.
    /// </summary>
    public static Error NeedFileSystemAccess(Error inner) =>
        DisplayError(ErrorMessages.FileSystemAccessNeeded, inner);

    /// <summary>
    /// Creates an error for when access to a specific path is denied.
    /// </summary>
    public static Error AccessToPathDenied(string path, Error inner) =>
        DisplayError($"{ErrorMessages.AccessToPathDeniedPrefix} {path} is denied", inner);

    /// <summary>
    /// Creates an error for when a path is empty or null.
    /// </summary>
    public static Error PathIsEmpty(Error inner) =>
       DisplayError(ErrorMessages.PathIsEmpty, inner);

    /// <summary>
    /// Creates an error for when a directory path is invalid.
    /// </summary>
    public static Error DirectoryInvalid(Error inner, string path) =>
        DisplayError($"{ErrorMessages.DirectoryInvalidPrefix} {path}", inner);

    /// <summary>
    /// Creates an error for when there's an issue getting files with a specific extension.
    /// </summary>
    public static Error GetFilesError(string extension, Error inner) =>
        DisplayError($"{ErrorMessages.ErrorGettingFilesPrefix} {extension}", inner);

    /// <summary>
    /// Creates an error for unauthorized access to a specific location.
    /// </summary>
    public static Error UnauthorisedAccess(string location, Error inner) =>
        DisplayError($"{ErrorMessages.UnauthorisedAccessPrefix} {location}", inner);

    /// <summary>
    /// Creates an error for when a file is not found at a specific path.
    /// </summary>
    public static Error FileNotFound(string path, Option<Error> inner) =>
        DisplayError($"{ErrorMessages.FileNotFoundPrefix} {path}", inner);

    /// <summary>
    /// Creates an error for when a directory is not found at a specific path.
    /// </summary>
    public static Error DirectoryNotFound(string path, Error inner) =>
        DisplayError($"{ErrorMessages.DirectoryNotFoundPrefix} {path}", inner);

    /// <summary>
    /// Creates an error for when there's an issue reading a specific file.
    /// </summary>
    public static Error ReadFileError(string filename, Error inner) =>
        DisplayError($"{ErrorMessages.ErrorReadingFilePrefix} {filename}", inner);

    /// <summary>
    /// Creates an error for when there's an issue moving a file or directory.
    /// </summary>
    public static Error UnableToMove(string path, Error inner) =>
        DisplayError($"{ErrorMessages.UnableToMovePrefix} {path}", inner);

    /// <summary>
    /// Creates an error for when there's an issue deleting a file.
    /// </summary>
    public static Error UnableToDelete(string fileName, Error inner) =>
        DisplayError($"{ErrorMessages.UnableToDeletePrefix} {fileName}", inner);

    /// <summary>
    /// Creates an error for when there's an issue rotating an image file.
    /// </summary>
    public static Error UnableToRotate(string fileName, Error inner) =>
       DisplayError($"{ErrorMessages.UnableToRotatePrefix} {fileName}{ErrorMessages.UnableToRotateSuffix}", inner);

    /// <summary>
    /// Creates an error for when there's an issue creating a directory.
    /// </summary>
    public static Error UnableToCreateDirectory(string directory, Error inner) =>
       DisplayError($"{ErrorMessages.UnableToCreateDirectoryPrefix} {directory}", inner);
}