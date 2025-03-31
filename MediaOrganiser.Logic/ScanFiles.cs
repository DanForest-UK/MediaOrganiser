using LanguageExt.Common;
using LanguageExt;
using System;
using System.IO;
using LanguageExt.Core;
using static LanguageExt.Prelude;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static MediaOrganiser.Core.Types;
using MediaOrganiser.Core;
using LanguageExt.Traits;

namespace MediaOrganiser.Logic
{
    public static class ScanFiles
    {
        static readonly Seq<string> ImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp"];
        static readonly Seq<string> VideoExtensions = [".mp4", ".avi", ".mov", ".wmv", ".mkv", ".flv", ".webm"];
        static readonly Seq<string> DocumentExtensions = [".pdf", ".docx", ".doc", ".txt", ".rtf"];

        /// <summary>
        /// Scans filder and gets list of file info
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IO<FileResponse> DoScanFiles(string path) =>
             (from extensions in IO.pure(ImageExtensions.Concat(VideoExtensions).Concat(DocumentExtensions))
              from getFilesResult in extensions.Map(ext => GetFilesWithExtension(path, ext))
                                               .ExtractUserErrors()
              from getFileInfoResult in getFilesResult.Succs
                                                      .Flatten()
                                                      .Map(CreateFileInfoAsync)
                                                      .ExtractUserErrors()
              select new FileResponse(
                  UserErrors: getFileInfoResult.UserErrors.Concat(getFilesResult.UserErrors),
                  Files: getFileInfoResult.Succs)).Safe();


        /// <summary>
        /// Get all files in a directory with a given extension
        /// </summary>
        static IO<Seq<string>> GetFilesWithExtension(string path, string extension) =>
            IO.lift(env => toSeq(Directory.GetFiles(path, $"*{extension}", SearchOption.AllDirectories)))
                .HandleUnauthorised(path)
                .HandleEmptyPath()
                .HandleInvalidDirectory(path)
                .HandleDirectoryNotFound(path)
                | @catch(e => e.Inner.IsEmpty(), error => IO.fail<Seq<string>>(AppErrors.GetFilesError(extension, error)));

        /// <summary>
        /// Adds file info for a file to the collection
        /// </summary>
        static IO<MediaInfo> CreateFileInfoAsync(string path) =>
            (from fileInfo in IO.lift(() => new FileInfo(path))
            from mediaInfo in fileInfo.Exists
                ? IO.lift(env =>
                {
                    var filename = Path.GetFileNameWithoutExtension(path);
                    var extension = Path.GetExtension(path).ToLower();

                    return new MediaInfo(
                        new FileId(Guid.NewGuid().GetHashCode()),
                        new FileName(filename),
                        new FullPath(path),
                        new Extension(extension),
                        new Size(fileInfo.Length),
                        new Date(fileInfo.LastWriteTime), // todo take earliest of all dates
                        Enumerable.Contains(ImageExtensions, extension)
                            ? FileCategory.Image
                            : Enumerable.Contains(VideoExtensions, extension)
                                ? FileCategory.Video
                                : Enumerable.Contains(DocumentExtensions, extension)
                                    ? FileCategory.Document
                                    : FileCategory.Unknown,
                        FileState.Undecided);
                })
               : IO.fail<MediaInfo>(AppErrors.FileNotFound(path, None))
            select mediaInfo).HandleUnauthorised(path)
            | @catch(e => IO.fail<MediaInfo>(AppErrors.ReadFileError(path, e)));
    }
}