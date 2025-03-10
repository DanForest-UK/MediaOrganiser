using System;
using System.Security.Cryptography.X509Certificates;
using LanguageExt;
using LanguageExt.ClassInstances;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using static SortPhotos.Core.UserErrors;

namespace SortPhotos.Core
{
    public static class Types
    {
        public record AppModel(
            Map<FileId, MediaInfo> Files,
            bool WorkInProgress = false,
            Option<FolderPath> CurrentFolder = default,
            Option<FileId> CurrentFile = default,
            bool CopyOnly = false,
            bool SortByYear = false);

        public record FileId(int Value) : IComparable<FileId>
        {
            // Implement IComparable for sorting in Map type
            public int CompareTo(FileId? other) =>
                other is null ? 1 : Value.CompareTo(other.Value);
        }

        public record FileName(string Value);
        public record FullPath(string Value);
        public record Extension(string Value);
        public record Size(long Value);
        public record Date(DateTime Value);
        public record FolderPath(string Value);

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
            FileState State);

        public record FileResponse(Seq<UserError> UserErrors, Seq<MediaInfo> Files);
        public record OrganiseResponse(Seq<UserError> UserErrors, bool success);
    }

    public static class AppErrors
    {
        public const int DisplayErrorCode = 303;

        public static Error DisplayError(string message, Error inner) =>
            Error.New(DisplayErrorCode, message, inner);

        public static Error ThereWasAProblem(Error inner) =>
            DisplayError("There was a problem", inner);

        public static Error NeedFileSystemAccess(Error inner) =>
            DisplayError("File access needs to be granted for this app in Privacy & Security -> File system", inner);

        public static Error AccessToPathDenied(string path, Error inner) =>
            DisplayError($"Access to: {path} is denied", inner);

        public static Error GetFilesError(string extension, Error inner) =>
            DisplayError($"Error getting files type: {extension}", inner);

        public static Error ReadFileError(string filename, Error inner) =>
            DisplayError($"Error reading file: {filename}", inner);

        public static Error UnableToMove(string fileName, string message, Error inner) =>
            DisplayError($"Unable to move/copy file {fileName}: {message}", inner);

        public static Error UnableToDelete(string fileName, string message, Error inner) =>
            DisplayError($"Unable to delete file {fileName}: {message}", inner);

        public static Error UnableToCreateDirectory(string directory, string message, Error inner) =>
           DisplayError($"Unable to create directory {directory}: {message}", inner);

        public static (Seq<UserError> User, Seq<Error> Unexpected) SeparateUserErrors(this Seq<Error> allErrors)
        {
            var separated = allErrors.Separate(err => err.Code == DisplayErrorCode);
            return (User: separated.Matched.Select(item => new UserError(item.Message)), Unexpected: separated.Unmatched);
        }
    }

    public static class UserErrors
    {
        public record UserError(string message);
    }
}