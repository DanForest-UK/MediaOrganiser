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
using static SortPhotos.Core.Extensions;

namespace SortPhotos.Logic
{
    public static class OrganiseFiles
    {
        public static IO<Seq<UserError>> DoOrganiseFiles(Seq<MediaInfo> files, string destinationBasePath, bool copyOnly) =>
            from keep in MoveFilesToKeep(files, destinationBasePath, copyOnly)
            from results in copyOnly
                ? IO.pure<Seq<UserError>>(default) 
                : DeleteFilesToBin(files)
            select results;

        /// <summary>
        /// Organizes files based on their state (Keep or Bin)
        /// </summary>
        static IO<Seq<UserError>> MoveFilesToKeep(Seq<MediaInfo> files, string destinationBasePath, bool copyOnly) =>
            from keep in IO.pure(files.Where(f => f.State == FileState.Keep).Map(f => MoveFile(f, destinationBasePath, copyOnly)))
            from result in keep.ExtractUserErrors()
            select result.UserErrors;

        static IO<Seq<UserError>> DeleteFilesToBin(Seq<MediaInfo> files) =>
            from bin in IO.pure(files.Where(f => f.State == FileState.Bin).Map(f => IO.lift(() => File.Delete(f.FullPath.Value))))
            from result in bin.ExtractUserErrors()
            select result.UserErrors;

        static string FolderName(FileCategory category) =>
            category switch
            {
                FileCategory.Image => "Images",
                FileCategory.Video => "Videos",
                FileCategory.Document => "Documents",
                _ => "Other"
            };

        static IO<Unit> MoveFile(MediaInfo file, string destinationBasePath, bool copyOnly) =>
            from directory in CreateDirectory(destinationBasePath, FolderName(file.Category), file.Date.Value.Year.ToString())
            from folderName in IO.pure(FolderName(file.Category))
            from targetDir in CreateDirectory(destinationBasePath, folderName, file.Date.Value.Year.ToString())
            from targetPath in IO.lift(() =>
            {
                var targetPath = Path.Combine(targetDir, file.FileName.Value + file.Extension.Value);
                if (copyOnly)
                {
                    File.Copy(file.FullPath.Value, targetPath, true);
                }
                else
                {
                    File.Move(file.FullPath.Value, targetPath, true);
                }
            })
            select unit;

        static IO<string> CreateDirectory(string destinationBasePath, string folderName, string year) =>
            from targetDir in IO.pure(Path.Combine(destinationBasePath, folderName, year))
            from _ in IO.lift(() => Directory.CreateDirectory(targetDir))
            select targetDir;       
    }
}