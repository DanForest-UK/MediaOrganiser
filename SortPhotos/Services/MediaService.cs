using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using MediaOrganizer.Logic;
using static SortPhotos.Core.Types;
using static LanguageExt.Prelude;

namespace SortPhotos.Logic
{
    public class MediaService
    {
        private List<MediaInfo> _allFiles = new List<MediaInfo>();
        private List<MediaInfo> _displayedFiles = new List<MediaInfo>();

        /// <summary>
        /// Scans the specified directory for media files
        /// </summary>
        public async Task<Either<Error, Seq<MediaInfo>>> ScanDirectoryAsync(string path)
        {
            var result = await ScanFiles.ScanFilesAsync(path);

            return result; // todo reinstate properly
            //return result.Match(
            //    Right: files =>
            //    {
            //        _allFiles = files.ToList();
            //        _displayedFiles = new List<MediaInfo>(_allFiles);
            //        return files;
            //    },
            //    Left: error => error);
            //);
        }

        /// <summary>
        /// Updates the state of a file
        /// </summary>
        public MediaInfo UpdateFileState(string fullPath, FileState newState)
        {
            // Find the file in the all files collection
            var fileIndex = _allFiles.FindIndex(f => f.FullPath == fullPath);
            if (fileIndex >= 0)
            {
                // Create updated file with new state
                var updatedFile = _allFiles[fileIndex] with { State = newState };

                // Update in collections
                _allFiles[fileIndex] = updatedFile;

                // Remove from displayed files
                _displayedFiles.RemoveAll(f => f.FullPath == fullPath);

                return updatedFile;
            }

            return null;
        }

        /// <summary>
        /// Get files that need to be processed
        /// </summary>
        public IEnumerable<MediaInfo> GetFilesToProcess()
        {
            return _allFiles;
        }

        /// <summary>
        /// Get displayed files
        /// </summary>
        public IEnumerable<MediaInfo> GetDisplayedFiles()
        {
            return _displayedFiles;
        }

        /// <summary>
        /// Organizes the files based on their state
        /// </summary>
        public async Task<Either<Error, int>> OrganizeFilesAsync(string destinationPath)
        {
            return await OrganiseFiles.OrganizeFilesAsync(toSeq(_allFiles), destinationPath); // todo why ToSeq() not working
        }

        /// <summary>
        /// Counts files marked for deletion
        /// </summary>
        public int CountFilesForDeletion()
        {
            return OrganiseFiles.CountFilesForDeletion(toSeq(_allFiles));
        }
    }
}