using LanguageExt;
using MediaOrganiser.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MediaOrganiser.Core.Types;
using static LanguageExt.Prelude;
using LanguageExt.Common;

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
        public async Task<Either<Error, OrganiseResponse>> OrganizeFilesAsync(string destinationPath)
        {
            try
            {
                var result = await Task.Run(() =>
                    OrganiseFiles.DoOrganiseFiles(
                        ObservableState.Current.Files.Values.ToSeq(),
                        destinationPath,
                        ObservableState.Current.CopyOnly,
                        ObservableState.Current.SortByYear,
                        ObservableState.Current.KeepParentFolder).Run());

                if (!result.Any())
                    ObservableState.ClearFiles();
                return new OrganiseResponse(result, result.IsEmpty);
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
    }
}