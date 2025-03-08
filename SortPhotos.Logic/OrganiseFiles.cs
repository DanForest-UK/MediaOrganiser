using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using SortPhotos.Core;
using static SortPhotos.Core.Types;

namespace SortPhotos.Logic
{
    public static class OrganiseFiles
    {
        /// <summary>
        /// Organizes files based on their state (Keep or Bin)
        /// </summary>
        public static async Task<Either<Error, int>> OrganizeFilesAsync(Seq<MediaInfo> files, string destinationBasePath)
        {
            try
            {
                int processedCount = 0;

                // Process files marked for keeping
                var keepFiles = files.Where(f => f.State == FileState.Keep).ToList();
                foreach (var file in keepFiles)
                {
                    string targetDir = Path.Combine(
                        destinationBasePath,
                        file.Category == FileCategory.Image ? "Images" : "Videos",
                        file.Date.Year.ToString());

                    // Create directory if it doesn't exist
                    Directory.CreateDirectory(targetDir);

                    string targetPath = Path.Combine(targetDir, $"{file.FileName}.{file.Extension}");

                    // Copy file to new location
                    File.Copy(file.FullPath, targetPath, true);
                    processedCount++;
                }

                // Process files marked for deletion
                var binFiles = files.Where(f => f.State == FileState.Bin).ToList();
                foreach (var file in binFiles)
                {
                    File.Delete(file.FullPath);
                    processedCount++;
                }

                return processedCount;
            }
            catch (Exception ex)
            {
                return AppErrors.ThereWasAProblem(Error.New(ex.Message));
            }
        }

        /// <summary>
        /// Counts files marked for deletion - todo move to logic
        /// </summary>
        public static int CountFilesForDeletion(Seq<MediaInfo> files)
        {
            return files.Count(f => f.State == FileState.Bin);
        }
    }
}