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
using static SortPhotos.Core.Types;
using SortPhotos.Core;
using LanguageExt.Traits;
using static SortPhotos.Core.UserErrors;

namespace SortPhotos.Logic
{
    public static class ScanFiles
    {
        private static readonly Seq<string> ImageExtensions = [ ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp" ];
        private static readonly Seq<string> VideoExtensions = [ ".mp4", ".avi", ".mov", ".wmv", ".mkv", ".flv", ".webm" ];


        public static IO<FileResponse> DoScanFiles(string path) =>
             (from extensions in IO.pure(ImageExtensions.Concat(VideoExtensions))
              from files in extensions.Map(ext => GetFilesWithExtension(path, ext)).Partition()
              from infos in files.Succs
                                 .Flatten()
                                 .Map(CreateFileInfoAsync)
                                 .Partition()
              let allErrors = files.Fails.Concat(infos.Fails)
              let separatedErrors = allErrors.SeparateUserErrors()
              from _ in separatedErrors.Unexpected.Traverse(IO.fail<FileResponse>)             
              select new FileResponse(separatedErrors.User, infos.Succs)).Safe();

        private static IO<Seq<string>> GetFilesWithExtension(string path, string extension) =>
            IO.lift(env => toSeq(Directory.GetFiles(path, $"*{extension}", SearchOption.AllDirectories)))
                | @catch(e => IO.fail<Seq<string>>(AppErrors.GetFilesError(extension, e))); // add specific unauthorised exct

        /// <summary>
        /// Adds file info for a file to the collection
        /// </summary>
        private static IO<MediaInfo> CreateFileInfoAsync(string path) =>
            IO.lift(env =>
            {
                var fileInfo = new FileInfo(path);
                if (!fileInfo.Exists) IO.fail<MediaInfo>(Error.New("File no longer exists"));

                var filename = Path.GetFileNameWithoutExtension(path);
                var extension = Path.GetExtension(path);

                return new MediaInfo(
                    filename,
                    path,
                    extension,
                    fileInfo.Length,
                    fileInfo.CreationTime,
                    Enumerable.Contains(ImageExtensions, extension)
                        ? FileCategory.Image
                        : FileCategory.Video,
                    FileState.Unprocessed);
            })
            | @catch(e => IO.fail<MediaInfo>(AppErrors.ReadFileError(path, e)));
    }
}