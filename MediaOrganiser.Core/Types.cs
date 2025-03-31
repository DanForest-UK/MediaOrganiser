using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using LanguageExt;
using LanguageExt.ClassInstances;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using static MediaOrganiser.Core.AppErrors;
using static MediaOrganiser.Core.AppErrors.ErrorMessages;

namespace MediaOrganiser.Core
{
    public static class Types
    {
        public record AppModel(
            Map<FileId, MediaInfo> Files,
            bool WorkInProgress,
            Option<FolderPath> CurrentFolder,
            Option<FileId> CurrentFile,
            CopyOnly CopyOnly,
            SortByYear SortByYear,
            KeepParentFolder KeepParentFolder);

        public record FileId(int Value) : IComparable<FileId>
        {
            // Implement IComparable for sorting in Map type
            public int CompareTo(FileId? other) =>
                other is null ? 1 : Value.CompareTo(other.Value);
        }

        // New types for better compile time safety
        public record FileName(string Value);
        public record FullPath(string Value);
        public record Extension(string Value);
        public record Size(long Value);
        public record Date(DateTime Value);
        public record FolderPath(string Value);
        public record CopyOnly(bool Value);
        public record SortByYear(bool Value);
        public record KeepParentFolder(bool Value);

        public enum FileCategory
        {
            Image,
            Video,
            Document,
            Unknown
        }

        public enum FileState
        {
            Keep,
            Bin,
            Undecided
        }

        public record MediaInfo(
            FileId Id,
            FileName FileName,
            FullPath FullPath,
            Extension Extension,
            Size Size,
            Date Date,
            FileCategory Category,
            FileState State,
            Rotation Rotation = Rotation.None);

        public enum Rotation
        {
            None = 0,
            Rotate90 = 90,
            Rotate180 = 180,
            Rotate270 = 270
        }

        public record FileResponse(Seq<UserError> UserErrors, Seq<MediaInfo> Files);
        public record OrganiseResponse(Seq<UserError> UserErrors, bool Success);
    }

    public static class AppErrors
    {
        // Type to distinguish expected errors from unexpected errors
        public record UserError(string Message);

        public const int DisplayErrorCode = 303;

        /// <summary>
        /// Common error message strings
        /// </summary>
        public static class ErrorMessages
        {
            public const string ThereWasAProblem = "There was a problem";
            public const string FileSystemAccessNeeded = "File access needs to be granted for this app in Privacy & Security -> File system";
            public const string AccessToPathDeniedPrefix = "Access to:";
            public const string ErrorGettingFilesPrefix = "Error getting files type:";
            public const string UnauthorisedAccessPrefix = "You do not have sufficient privalages for:";
            public const string FileNotFoundPrefix = "File not found:";
            public const string DirectoryNotFoundPrefix = "Directory not found:";
            public const string ErrorReadingFilePrefix = "Error reading file:";
            public const string UnableToMovePrefix = "Unable to move/copy file:";
            public const string UnableToDeletePrefix = "Unable to delete file";
            public const string UnableToRotateSuffix = ", it was copied in its original orientation";
            public const string UnableToRotatePrefix = "Unable to rotate file";
            public const string UnableToCreateDirectoryPrefix = "Unable to create directory";
            public const string PathIsEmpty = "Path is empty";
            public const string DirectoryInvalidPrefix = "Directory is invalid:";
        }

        public static Error DisplayError(string message, Option<Error> inner) =>
            inner.Match(
                Some: inn => Error.New(DisplayErrorCode, message, inn),
                None: () => Error.New(DisplayErrorCode, message));

        public static Error ThereWasAProblem(Error inner) =>
            DisplayError(ErrorMessages.ThereWasAProblem, inner);

        public static Error NeedFileSystemAccess(Error inner) =>
            DisplayError(FileSystemAccessNeeded, inner);

        public static Error AccessToPathDenied(string path, Error inner) =>
            DisplayError($"{AccessToPathDeniedPrefix} {path} is denied", inner);

        public static Error PathIsEmpty(Error inner) =>
           DisplayError(ErrorMessages.PathIsEmpty, inner);

        public static Error DirectoryInvalid(Error inner, string path) =>
            DisplayError($"{DirectoryInvalidPrefix} {path}", inner);

        public static Error GetFilesError(string extension, Error inner) =>
            DisplayError($"{ErrorGettingFilesPrefix} {extension}", inner);

        public static Error UnauthorisedAccess(string location, Error inner) =>
            DisplayError($"{UnauthorisedAccessPrefix} {location}", inner);

        public static Error FileNotFound(string path, Option<Error> inner) =>
            DisplayError($"{FileNotFoundPrefix} {path}", inner);

        public static Error DirectoryNotFound(string path, Error inner) =>
            DisplayError($"{DirectoryNotFoundPrefix} {path}", inner);

        public static Error ReadFileError(string filename, Error inner) =>
            DisplayError($"{ErrorReadingFilePrefix} {filename}", inner);

        public static Error UnableToMove(string path, Error inner) =>
            DisplayError($"{UnableToMovePrefix} {path}", inner);

        public static Error UnableToDelete(string fileName, Error inner) =>
            DisplayError($"{UnableToDeletePrefix} {fileName}", inner);

        public static Error UnableToRotate(string fileName, Error inner) =>
           DisplayError($"{UnableToRotatePrefix} {fileName}{UnableToRotateSuffix}", inner);

        public static Error UnableToCreateDirectory(string directory, Error inner) =>
           DisplayError($"{UnableToCreateDirectoryPrefix} {directory}", inner);

        public static (Seq<UserError> User, Seq<Error> Unexpected) SeparateUserErrors(this Seq<Error> allErrors)
        {
            // Log errors before converting to user safe errors
            allErrors.Iter(e =>
            {
                Debug.WriteLine(e.Message);
                e.Exception.IfSome(ex => Debug.Write(ex));
                e.Inner.IfSome(inner => Debug.Write(inner.Exception));
            });

            var separated = allErrors.Separate(err => err.Code == DisplayErrorCode);
            return (User: separated.Matched.Select(item => new UserError(item.Message)), Unexpected: separated.Unmatched);
        }
    }
}