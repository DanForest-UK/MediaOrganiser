using static LanguageExt.Prelude;
using LanguageExt;

namespace MediaOrganiser.Domain;

/// <summary>
/// Main state model for the app
/// </summary>
/// <param name="Files">Collection of files to sort</param>
/// <param name="WorkInProgress">Is an operation in progress</param>
/// <param name="CurrentFolder">Current folder</param>
/// <param name="CurrentFile">Current file ID</param>
/// <param name="CopyOnly">Copy files when organise instead of move</param>
/// <param name="SortByYear">Sort into folders by year</param>
/// <param name="KeepParentFolder">Create top level folder in new folder heirachy</param>
public record AppModel(
            Map<FileId, MediaInfo> Files,
            bool WorkInProgress,
            Option<FolderPath> CurrentFolder,
            Option<FileId> CurrentFile,
            CopyOnly CopyOnly,
            SortByYear SortByYear,
            KeepParentFolder KeepParentFolder)
{

    /// <summary>
    /// Empty app model
    /// </summary>
    public static AppModel Empty => new(
        [],
        false,
        None,
        None,
        new CopyOnly(false),
        new SortByYear(false),
        new KeepParentFolder(false));

    /// <summary>
    /// Counts how many files are to be deleted
    /// </summary>
    /// <returns></returns>
    public int CountFilesForDeletion() =>
      Files.Values.Count(f => f.State == FileState.Bin);

    /// <summary>
    /// Add new set of files into an app model
    /// </summary>
    public AppModel SetFiles(Seq<MediaInfo> files)
    {
        // Filter and assign IDs
        files = toSeq(files.Where(s => s.Category != FileCategory.Unknown)
                          .Select((s, i) => s with { Id = new FileId(i + 1) }));

        var fileMap = (from f in files
                       select (f.Id, f)).ToMap();

        return this with
        {
            Files = fileMap,
            CurrentFile = files.Length > 0
                ? files.First().Id
                : None
        };
    }

    /// <summary>
    /// Rotate the current image - internal implementation
    /// </summary> 
    Option<AppModel> RotateCurrentImage(Rotation direction) =>
        CurrentFile.Map(
            currentId =>
            {
                if (Files.ContainsKey(currentId))
                {
                    var file = Files[currentId];

                    var newRotation = direction == Rotation.Rotate90
                        ? (Rotation)(((int)file.Rotation + 90) % 360)
                        : direction == Rotation.Rotate270
                            ? (Rotation)(((int)file.Rotation + 270) % 360)
                            : file.Rotation;

                    return this with
                    {
                        Files = Files.AddOrUpdate(currentId, file with { Rotation = newRotation })
                    };
                }
                return this;
            });

    /// <summary>
    /// Rotate the current image
    /// </summary> 
    public AppModel TryRotateCurrentImage(Rotation direction) =>
        RotateCurrentImage(direction).IfNone(this);

    /// <summary>
    /// Move to the next file
    /// </summary>
    Option<AppModel> MoveToNextFile() =>
        from currentFile in CurrentFile
        let keys = Files.Keys.ToList()
        let index = keys.FindIndex(k => k.Value == currentFile.Value)
        where index >= 0 && index < keys.Count - 1
        select this with { CurrentFile = keys[index + 1] };

    public AppModel TryMoveToNextFile() =>
        MoveToNextFile().IfNone(this);

    /// <summary>
    /// Move to the previous file - internal implementation
    /// </summary>
    Option<AppModel> TryMoveToPreviousFile() =>
        CurrentFile.Map(
            currentId =>
            {
                var keys = Files.Keys.ToList();
                var index = keys.FindIndex(k => k.Value == currentId.Value);

                if (index > 0)
                {
                    return this with { CurrentFile = keys[index - 1] };
                }
                return this;
            });

    /// <summary>
    /// Move to the previous file
    /// </summary>
    public AppModel MoveToPreviousFile() =>
        TryMoveToPreviousFile().IfNone(this);

    /// <summary>
    /// Update file state - internal implementation
    /// </summary>
    Option<AppModel> UpdateFileState(FileState state) =>
        CurrentFile.Map(
            currentId =>
            {
                if (Files.ContainsKey(currentId))
                {
                    return this with
                    {
                        Files = Files.AddOrUpdate(currentId, Files[currentId] with { State = state })
                    };
                }
                return this;
            });

    /// <summary>
    /// Update file state
    /// </summary>
    public AppModel TryUpdateFileState(FileState state) =>
        UpdateFileState(state).IfNone(this);

    /// <summary>
    /// Update file name - internal implementation
    /// </summary>
    Option<AppModel> UpdateFilename(string newFilename) =>
        CurrentFile.Map(
            currentId =>
            {
                if (Files.ContainsKey(currentId))
                {
                    return this with
                    {
                        Files = Files.AddOrUpdate(currentId, Files[currentId] with { FileName = new FileName(newFilename) })
                    };
                }
                return this;
            });

    /// <summary>
    /// Update file name
    /// </summary>
    public AppModel TryUpdateFilename(string newFilename) =>
        UpdateFilename(newFilename).IfNone(this);
}