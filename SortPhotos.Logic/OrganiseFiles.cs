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
using System.Drawing;

namespace SortPhotos.Logic
{
    public static class OrganiseFiles
    {
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

        static IO<Seq<UserError>> DeleteFilesToBin(Seq<MediaInfo> files) =>
            from bin in IO.pure(files.Where(f => f.State == FileState.Bin)
                .Map(f => DeleteFile(f.FullPath.Value)))
            from result in bin.ExtractUserErrors()
            select result.UserErrors;

        static IO<Unit> DeleteFile(string path) =>
            IO.lift(() => File.Delete(path))
            .HandleUnauthorised(path)
            | @catch(err => IO.fail<Unit>(AppErrors.UnableToMove(err.Message, err)));

        static string FolderName(FileCategory category) =>
            category switch
            {
                FileCategory.Image => "Images",
                FileCategory.Video => "Videos",
                FileCategory.Document => "Documents",
                _ => "Other"
            };

        static IO<Unit> MoveFile(
            MediaInfo file,
            string destinationBasePath,
            CopyOnly copyOnly,
            SortByYear sortByYear,
            KeepParentFolder keepParentFolder) =>
                (from targetDir in GetTargetDirectory(file, destinationBasePath, FolderName(file.Category), sortByYear, keepParentFolder)
                 from targetPath in IO.lift(() =>
                 {
                     int counter = 1;
                   
                     var targetPath = Path.Combine(targetDir, file.FileName.Value + file.Extension.Value);

                     while (File.Exists(targetPath))
                     {
                         targetPath = Path.Combine(targetDir, file.FileName.Value + counter++ + file.Extension.Value);
                     }

                     // If rotation is applied and file is an image, save with rotation
                     if (file.Rotation != Rotation.None && file.Category == FileCategory.Image)
                     {
                         // Load the image, apply rotation and save to destination
                         using (var image = Image.FromFile(file.FullPath.Value))
                         {
                             // Apply rotation
                             RotateFlipType rotateFlip = file.Rotation switch
                             {
                                 Rotation.Rotate90 => RotateFlipType.Rotate90FlipNone,
                                 Rotation.Rotate180 => RotateFlipType.Rotate180FlipNone,
                                 Rotation.Rotate270 => RotateFlipType.Rotate270FlipNone,
                                 _ => RotateFlipType.RotateNoneFlipNone
                             };

                             // Create a new bitmap with rotation applied
                             using (var rotatedImage = new System.Drawing.Bitmap(image))
                             {
                                 rotatedImage.RotateFlip(rotateFlip);
                                 rotatedImage.Save(targetPath);
                             }
                         }

                         // If we're not in copy mode, delete the original
                         if (!copyOnly.Value)
                         {
                             File.Delete(file.FullPath.Value);
                         }
                     }
                     else
                     {
                         // Normal copy/move for non-rotated images or non-image files
                         if (copyOnly.Value)
                         {
                             File.Copy(file.FullPath.Value, targetPath, true);
                         }
                         else
                         {
                             File.Move(file.FullPath.Value, targetPath, true);
                         }
                     }
                 })
                 select unit)
                .HandleUnauthorised(file.FullPath.Value)
                .HandleFileNotFound(file.FullPath.Value)
                .HandleDirectoryNotFound(file.FullPath.Value)
                | @catch(err => IO.fail<Unit>(AppErrors.UnableToMove(err.Message, err)));

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
            | @catch(err => IO.fail<string>(AppErrors.UnableToCreateDirectory(destinationBasePath, err.Message, err)));
        }
    }
}