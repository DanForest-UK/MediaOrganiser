﻿using LanguageExt.Common;
using LanguageExt;
using System;
using System.IO;
using LanguageExt.Core;
using static LanguageExt.Prelude;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static SortPhotos.Core.Types;
using SortPhotos.Core;
using LanguageExt.Traits;
using static SortPhotos.Core.UserErrors;

namespace SortPhotos.Logic
{
    public static class ScanFiles
    {
        static readonly Seq<string> ImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp"];
        static readonly Seq<string> VideoExtensions = [".mp4", ".avi", ".mov", ".wmv", ".mkv", ".flv", ".webm"];
        static readonly Seq<string> DocumentExtensions = [".pdf", ".docx", ".doc", ".txt"];

        public static IO<FileResponse> DoScanFiles(string path) =>
             (from extensions in IO.pure(ImageExtensions.Concat(VideoExtensions).Concat(DocumentExtensions))
              from files in extensions.Map(ext => GetFilesWithExtension(path, ext)).Partition()
              from infos in files.Succs
                                 .Flatten()
                                 .Map(CreateFileInfoAsync)
                                 .Partition()
              let allErrors = files.Fails.Concat(infos.Fails)
              let separatedErrors = allErrors.SeparateUserErrors()
              from _ in separatedErrors.Unexpected.Traverse(IO.fail<FileResponse>)
              select new FileResponse(separatedErrors.User, infos.Succs)).Safe();

        static IO<Seq<string>> GetFilesWithExtension(string path, string extension) =>
            IO.lift(env => toSeq(Directory.GetFiles(path, $"*{extension}", SearchOption.AllDirectories)))
                | @catch(e => IO.fail<Seq<string>>(AppErrors.GetFilesError(extension, e))); // add specific unauthorised exct

        /// <summary>
        /// Adds file info for a file to the collection
        /// </summary>
        static IO<MediaInfo> CreateFileInfoAsync(string path) =>
            IO.lift(env =>
            {
                var fileInfo = new FileInfo(path);
                if (!fileInfo.Exists) IO.fail<MediaInfo>(Error.New("File no longer exists"));

                var filename = Path.GetFileNameWithoutExtension(path);
                var extension = Path.GetExtension(path);

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
                    FileState.Unprocessed);
            })
            | @catch(e => IO.fail<MediaInfo>(AppErrors.ReadFileError(path, e)));
    }
}