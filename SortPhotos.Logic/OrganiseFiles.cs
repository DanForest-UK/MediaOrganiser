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
        public static IO<Seq<UserError>> DoOrganiseFiles(Seq<MediaInfo> files, string destinationBasePath, bool copyOnly, bool sortByYear = false) =>
            from moveErrors in MoveFilesToKeep(files, destinationBasePath, copyOnly, sortByYear)
            from binErrors in copyOnly
                ? IO.pure<Seq<UserError>>(default)
                : DeleteFilesToBin(files)
            select binErrors.Concat(moveErrors);

        /// <summary>
        /// Organizes files based on their state (Keep or Bin)
        /// </summary>
        static IO<Seq<UserError>> MoveFilesToKeep(Seq<MediaInfo> files, string destinationBasePath, bool copyOnly, bool sortByYear) =>
            from keep in IO.pure(files.Where(f => f.State == FileState.Keep)
                .Map(f => MoveFile(f, destinationBasePath, copyOnly, sortByYear)))
            from result in keep.ExtractUserErrors()
            select result.UserErrors;

        static IO<Seq<UserError>> DeleteFilesToBin(Seq<MediaInfo> files) =>
            from bin in IO.pure(files.Where(f => f.State == FileState.Bin)
                .Map(f => DeleteFile(f.FullPath.Value)))
            from result in bin.ExtractUserErrors()
            select result.UserErrors;

        static IO<Unit> DeleteFile(string path) =>
            IO.lift(() => File.Delete(path))
            .HandleUnauthorised(path)
            | @catch(err => IO.fail<Unit>(AppErrors.UnableToMove(path, err.Message, err)));

        static string FolderName(FileCategory category) =>
            category switch
            {
                FileCategory.Image => "Images",
                FileCategory.Video => "Videos",
                FileCategory.Document => "Documents",
                _ => "Other"
            };

        static IO<Unit> MoveFile(MediaInfo file, string destinationBasePath, bool copyOnly, bool sortByYear) =>
            (from folderName in IO.pure(FolderName(file.Category))
             from targetDir in sortByYear
                ? CreateDirectory(destinationBasePath, folderName, file.Date.Value.Year.ToString())
                : CreateDirectory(destinationBasePath, folderName)
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
             select unit)
            .HandleUnauthorised(file.FullPath.Value)
            .HandleFileNotFound(file.FullPath.Value)
            .HandleDirectoryNotFound(file.FullPath.Value)
            | @catch(err => IO.fail<Unit>(AppErrors.UnableToMove(file.FullPath.Value, err.Message, err)));

        static IO<string> CreateDirectory(string destinationBasePath, string folderName, string year = "") =>
            (from targetDir in IO.pure(!year.HasValue()
                ? Path.Combine(destinationBasePath, folderName)
                : Path.Combine(destinationBasePath, folderName, year))
             from _ in IO.lift(() => Directory.CreateDirectory(targetDir)).HandleUnauthorised(targetDir)
             .HandleDirectoryNotFound(targetDir)
                | @catch(err => IO.fail<DirectoryInfo>(AppErrors.UnableToCreateDirectory(targetDir, err.Message, err)))
             select targetDir);
    }
}