using LanguageExt;
using LanguageExt.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaOrganiser.Domain;

public static class AppErrors
{
    public static Error DisplayError(string message, Option<Error> inner) =>
        inner.Match(
            Some: inn => Error.New(UserError.DisplayErrorCode, message, inn),
            None: () => Error.New(UserError.DisplayErrorCode, message));

    public static Error ThereWasAProblem(Error inner) =>
        DisplayError(ErrorMessages.ThereWasAProblem, inner);

    public static Error NeedFileSystemAccess(Error inner) =>
        DisplayError(ErrorMessages.FileSystemAccessNeeded, inner);

    public static Error AccessToPathDenied(string path, Error inner) =>
        DisplayError($"{ErrorMessages.AccessToPathDeniedPrefix} {path} is denied", inner);

    public static Error PathIsEmpty(Error inner) =>
       DisplayError(ErrorMessages.PathIsEmpty, inner);

    public static Error DirectoryInvalid(Error inner, string path) =>
        DisplayError($"{ErrorMessages.DirectoryInvalidPrefix} {path}", inner);

    public static Error GetFilesError(string extension, Error inner) =>
        DisplayError($"{ErrorMessages.ErrorGettingFilesPrefix} {extension}", inner);

    public static Error UnauthorisedAccess(string location, Error inner) =>
        DisplayError($"{ErrorMessages.UnauthorisedAccessPrefix} {location}", inner);

    public static Error FileNotFound(string path, Option<Error> inner) =>
        DisplayError($"{ErrorMessages.FileNotFoundPrefix} {path}", inner);

    public static Error DirectoryNotFound(string path, Error inner) =>
        DisplayError($"{ErrorMessages.DirectoryNotFoundPrefix} {path}", inner);

    public static Error ReadFileError(string filename, Error inner) =>
        DisplayError($"{ErrorMessages.ErrorReadingFilePrefix} {filename}", inner);

    public static Error UnableToMove(string path, Error inner) =>
        DisplayError($"{ErrorMessages.UnableToMovePrefix} {path}", inner);

    public static Error UnableToDelete(string fileName, Error inner) =>
        DisplayError($"{ErrorMessages.UnableToDeletePrefix} {fileName}", inner);

    public static Error UnableToRotate(string fileName, Error inner) =>
       DisplayError($"{ErrorMessages.UnableToRotatePrefix} {fileName}{ErrorMessages.UnableToRotateSuffix}", inner);

    public static Error UnableToCreateDirectory(string directory, Error inner) =>
       DisplayError($"{ErrorMessages.UnableToCreateDirectoryPrefix} {directory}", inner);    
}
