using LanguageExt;
using SortPhotos.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SortPhotos.Core.Types;
using static LanguageExt.Prelude;
using LanguageExt.Common;
using MusicTools.Logic;

namespace MediaOrganiser.Services
{
    public class MediaService
    {
        /// <summary>
        /// Scans the specified directory for media files
        /// </summary>
        public async Task<Either<Error, FileResponse>> ScanDirectoryAsync(string path)
        {
            try
            {
                var result = await Task.Run(() => ScanFiles.DoScanFiles(path).Run());
                ObservableState.SetFiles(result.Files);
                return result!;
            }
            catch (ManyExceptions many)
            {
                foreach (var ex in many)
                    Debug.WriteLine(ex.Message);

                return many.Errors.First().ToError();
            }
            catch (ErrorException e)
            {
                return e.ToError();
            }
        }

        /// <summary>
        /// Organizes the files based on their state
        /// </summary>
        public async Task<Either<Error, int>> OrganizeFilesAsync(string destinationPath) =>
            await OrganiseFiles.OrganizeFilesAsync(toSeq(ObservableState.Current.Files.Values), destinationPath);

        /// <summary>
        /// Counts files marked for deletion
        /// </summary>
        public int CountFilesForDeletion() =>
            OrganiseFiles.CountFilesForDeletion(toSeq(ObservableState.Current.Files.Values));
    }
}