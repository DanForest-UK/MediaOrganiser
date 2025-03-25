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
using static MediaOrganiser.Core.Extensions;
using System.Drawing;
using static MediaOrganiser.Core.AppErrors;

namespace MediaOrganiser.Logic
{
    public static class OrganiseFiles
    {
        /// <summary>
        /// Organises files into folders moving or copying as per settings
        /// </summary>
        /// <param name="files"></param>
        /// <param name="destinationBasePath">Where the files will be organised to</param>
        /// <param name="copyOnly">Do not delete original files</param>
        /// <param name="sortByYear">Sort files into a folder by year</param>
        /// <param name="keepParentFolder">Recreate top level of original directory structure</param>
        /// <returns></returns>
        public static IO<Seq<UserError>> DoOrganiseFiles(
            Seq<MediaInfo> files,
            string destinationBasePath,
            CopyOnly copyOnly,
            SortByYear sortByYear,
            KeepParentFolder keepParentFolder) =>
                from moveErrors in MoveFilesToKeep(files, destinationBasePath, copyOnly, sortByYear, keepParentFolder)
                from binErrors in copyOnly.Value
                    ? IO.pure<Seq<UserError>>(default)
                    : DeleteFilesToBin(files)
                select binErrors.Concat(moveErrors);

        /// <summary>
        /// Organizes files based on their state (Keep or Bin)
        /// </summary>
        static IO<Seq<UserError>> MoveFilesToKeep(
            Seq<MediaInfo> files,
            string destinationBasePath,
            CopyOnly copyOnly,
            SortByYear sortByYear,
            KeepParentFolder keepParentFolder) =>
                from keep in IO.pure(files.Where(f => f.State == FileState.Keep)
                    .Map(f => MoveFile(f, destinationBasePath, copyOnly, sortByYear, keepParentFolder)))
                from result in keep.ExtractUserErrors()
                select result.UserErrors;

        /// <summary>
        /// Delete files tagged for binning
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        static IO<Seq<UserError>> DeleteFilesToBin(Seq<MediaInfo> files) =>
            from bin in IO.pure(files.Where(f => f.State == FileState.Bin)
                .Map(f => DeleteFile(f.FullPath.Value)))
            from result in bin.ExtractUserErrors()
            select result.UserErrors;

        /// <summary>
        /// Delete a single file
        /// </summary>
        static IO<Unit> DeleteFile(string path) =>
            IO.lift(() => File.Delete(path))
            .HandleUnauthorised(path)
            | @catch(err => IO.fail<Unit>(AppErrors.UnableToDelete(path, err)));

        static string FolderName(FileCategory category) =>
            category switch
            {
                FileCategory.Image => "Images",
                FileCategory.Video => "Videos",
                FileCategory.Document => "Documents",
                _ => "Other"
            };

        /// <summary>
        /// Move or copy a single file
        /// </summary>
        static IO<Unit> MoveFile(
            MediaInfo file,
            string destinationBasePath,
            CopyOnly copyOnly,
            SortByYear sortByYear,
            KeepParentFolder keepParentFolder) =>
                (from targetDir in GetTargetDirectory(file, destinationBasePath, FolderName(file.Category), sortByYear, keepParentFolder)
                 from targetPath in GetUniquePath(file, targetDir)
                 from shouldRotate in IO.pure(
                       file.Rotation != Rotation.None &&
                       file.Category == FileCategory.Image)
                 from _ in shouldRotate
                    ? Runtime.RotateImage(file, targetPath)
                      | @catch(err => // If we fail to rotate, we do a normal move but still log an error
                            from _1 in CopyFile(file.FullPath.Value, targetPath)
                            from _2 in IO.fail<Unit>(UnableToRotate(file.FullPath.Value, err))
                            select unit)
                     : CopyFile(file.FullPath.Value, targetPath)
                 from _1 in IO.lift(() =>
                 {
                     if (!copyOnly.Value)                    
                         File.Delete(file.FullPath.Value);                     
                 })                
                 select unit)
                .HandleUnauthorised(file.FullPath.Value)
                .HandleFileNotFound(file.FullPath.Value)
                .HandleDirectoryNotFound(file.FullPath.Value)
                | @catch(err => IO.fail<Unit>(UnableToMove(file.FullPath.Value, err)));

        /// <summary>
        /// Normal copy for files that do not need rotating
        /// </summary>
        static IO<Unit> CopyFile(string path, string targetPath) =>
            IO.lift(() => File.Copy(path, targetPath, true));
 
        // Make sure no filename clashes
        static IO<string> GetUniquePath(MediaInfo file, string targetDir) =>
            IO.lift(() =>
            {
                var counter = 1;

                var targetPath = Path.Combine(targetDir, file.FileName.Value + file.Extension.Value);

                while (File.Exists(targetPath))
                {
                    targetPath = Path.Combine(targetDir, file.FileName.Value + counter++ + file.Extension.Value);
                }
                return targetDir;
            });

        /// <summary>
        /// Determines and creates the target directory based on configuration and file details
        /// </summary>
        static IO<string> GetTargetDirectory(
            MediaInfo file,
            string destinationBasePath,
            string categoryFolder,
            SortByYear sortByYear,
            KeepParentFolder keepParentFolder)
        {
            return IO.lift(() =>
            {
                var pathComponents = new List<string> { destinationBasePath, categoryFolder };

                // Add parent folder name if enabled
                if (keepParentFolder.Value)
                    pathComponents.Add(new FileInfo(file.FullPath.Value)?.Directory?.Name!);

                // Add year if sorting by year is enabled
                if (sortByYear.Value)
                    pathComponents.Add(file.Date.Value.Year.ToString());

                // Create the target directory
                var targetDir = Path.Combine(pathComponents.ToArray());
                Directory.CreateDirectory(targetDir);

                return targetDir;
            })
            .HandleUnauthorised(destinationBasePath)
            .HandleDirectoryNotFound(destinationBasePath)
            | @catch(err => IO.fail<string>(UnableToCreateDirectory(destinationBasePath, err)));
        }
    }
}