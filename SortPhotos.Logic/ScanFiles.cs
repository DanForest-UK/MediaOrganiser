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

namespace MediaOrganizer.Logic
{
    public static class ScanFiles
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp" };
        private static readonly string[] VideoExtensions = { ".mp4", ".avi", ".mov", ".wmv", ".mkv", ".flv", ".webm" };

        /// <summary>
        /// Scans for image and video files and returns file information
        /// </summary>
        public static async Task<Either<Error, Seq<MediaInfo>>> ScanFilesAsync(string path)
        {
            try
            {
                var list = new List<MediaInfo>();

                // Get all image files
                foreach (var extension in ImageExtensions)
                {
                    var imageFiles = await GetFilesWithExtensionAsync(path, extension);
                    foreach (var file in imageFiles)
                    {
                        await AddFileInfo(file, list, FileCategory.Image);
                    }
                }

                // Get all video files
                foreach (var extension in VideoExtensions)
                {
                    var videoFiles = await GetFilesWithExtensionAsync(path, extension);
                    foreach (var file in videoFiles)
                    {
                        await AddFileInfo(file, list, FileCategory.Video);
                    }
                }

                // Sort by date (newest first)
                return toSeq(from item in list
                             orderby item.Date
                             select item);
            }
            catch (UnauthorizedAccessException)
            {
                return AppErrors.NeedFileSystemAccess; // todo check if error message appropriate
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return AppErrors.ThereWasAProblem;
            }
        }

        /// <summary>
        /// Gets files with the specified extension from a directory
        /// </summary>
        private static Task<IEnumerable<string>> GetFilesWithExtensionAsync(string path, string extension)
        {
            return Task.Run(() =>
            {
                try
                {
                    return Directory.GetFiles(path, $"*{extension}", SearchOption.AllDirectories);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting files with extension {extension}: {ex.Message}");
                    return Enumerable.Empty<string>();
                }
            });
        }

        /// <summary>
        /// Adds file info for a file to the collection
        /// </summary>
        private static Task AddFileInfo(string path, List<MediaInfo> list, FileCategory category)
        {
            return Task.Run(() =>
            {
                try
                {
                    var fileInfo = new FileInfo(path);
                    if (!fileInfo.Exists) return;

                    var filename = Path.GetFileNameWithoutExtension(path);
                    var extension = Path.GetExtension(path);

                    list.Add(new MediaInfo(
                        filename,
                        path,
                        extension,
                        fileInfo.Length,
                        fileInfo.CreationTime,
                        category,
                        FileState.Unprocessed));
                   
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error reading file info for {path}: {ex.Message}");
                }
            });
        }
    }
}