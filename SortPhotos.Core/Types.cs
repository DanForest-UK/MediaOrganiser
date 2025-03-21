﻿using System;
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

        public static Error UnauthorisedAccess(string location, Error inner) =>
            DisplayError($"You do not have sufficient privalages for: {location}", inner);

        public static Error FileNotFound(string path, Error inner) =>
            DisplayError($"File not found: {path}", inner);

        public static Error DirectoryNotFound(string path, Error inner) =>
            DisplayError($"Directory not found: {path}", inner);

        public static Error ReadFileError(string filename, Error inner) =>
            DisplayError($"Error reading file: {filename}", inner);

        public static Error UnableToMove(string message, Error inner) =>
            DisplayError($"Unable to move/copy file: {message}", inner);

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