using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using MediaOrganizer.Logic;
using static SortPhotos.Core.Types;
using static LanguageExt.Prelude;
using System.Diagnostics;

namespace SortPhotos.Logic
{
    public class MediaService
    {
        private List<MediaInfo> allFiles = new List<MediaInfo>();
        private List<MediaInfo> displayedFiles = new List<MediaInfo>();

        /// <summary>
        /// Scans the specified directory for media files
        /// </summary>
        public async Task<Either<Error, FileResponse>> ScanDirectoryAsync(string path)
        {
            try
            {

                var result = ScanFiles.DoScanFiles(path).Run();
                allFiles.AddRange(result.Files);
                displayedFiles.AddRange(result.Files);
                return result!;
            }
            catch (ManyExceptions many)
            {
                foreach (var ex in many)
                {
                    Debug.WriteLine(ex.Message);
                }
                return many.Errors.First().ToError();

            }
            catch (ErrorException e)
            {
                return e.ToError();
            }
        }


        /// <summary>
        /// Updates the state of a file
        /// </summary>
        public MediaInfo UpdateFileState(string fullPath, FileState newState)
        {
            // Find the file in the all files collection
            var fileIndex = allFiles.FindIndex(f => f.FullPath == fullPath);
            if (fileIndex >= 0)
            {
                // Create updated file with new state
                var updatedFile = allFiles[fileIndex] with { State = newState };

                // Update in collections
                allFiles[fileIndex] = updatedFile;

                // Remove from displayed files
                displayedFiles.RemoveAll(f => f.FullPath == fullPath);

                return updatedFile;
            }

            return null;
        }

        /// <summary>
        /// Get files that need to be processed
        /// </summary>
        public IEnumerable<MediaInfo> GetFilesToProcess()
        {
            return allFiles;
        }

        /// <summary>
        /// Get displayed files
        /// </summary>
        public IEnumerable<MediaInfo> GetDisplayedFiles()
        {
            return displayedFiles;
        }

        /// <summary>
        /// Organizes the files based on their state
        /// </summary>
        public async Task<Either<Error, int>> OrganizeFilesAsync(string destinationPath)
        {
            return await OrganiseFiles.OrganizeFilesAsync(toSeq(allFiles), destinationPath); // todo why ToSeq() not working
        }

        /// <summary>
        /// Counts files marked for deletion
        /// </summary>
        public int CountFilesForDeletion()
        {
            return OrganiseFiles.CountFilesForDeletion(toSeq(allFiles));
        }
    }
}