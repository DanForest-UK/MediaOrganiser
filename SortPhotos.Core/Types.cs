using System;
using LanguageExt;
using LanguageExt.ClassInstances;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using static SortPhotos.Core.UserErrors;

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

        public record FileResponse(Seq<UserError> UserErrors, Seq<MediaInfo> Files);
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
