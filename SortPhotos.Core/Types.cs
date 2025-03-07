using System;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace SortPhotos.Core
{
    public static class Types
    {
        public record AppData(
            MediaInfo[] Files);

        public enum FileCategory
        {
            Image,
            Video,
            Unknown
        }

        public enum FileState
        {
            Keep,
            Bin,
            Unprocessed
        }

        public record MediaInfo(
            string FileName,
            string FullPath,
            string Extension,
            long Size,
            DateTime Date,
            FileCategory Category,
            FileState State);
    }

    public static class AppErrors
    {
        public const int DisplayErrorCode = 303;

        public static Error DispayError(string message) =>
            Error.New(DisplayErrorCode, message);

        public static readonly Error ThereWasAProblem =
            DispayError("There was a problem");

        public static readonly Error NeedFileSystemAccess =
            DispayError("File access needs to be granted for this app in Privacy & Security -> File system");

        public static Error AccessToPathDenied(string path) =>
            DispayError($"Access to: {path} is denied");
    }
}
