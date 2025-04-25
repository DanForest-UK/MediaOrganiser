using static LanguageExt.Prelude;
using LanguageExt;

namespace MediaOrganiser.Domain;

public record AppModel(
            Map<FileId, MediaInfo> Files,
            bool WorkInProgress,
            Option<FolderPath> CurrentFolder,
            Option<FileId> CurrentFile,
            CopyOnly CopyOnly,
            SortByYear SortByYear,
            KeepParentFolder KeepParentFolder)

{
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
    /// Rotate the current image
    /// </summary> 
    public AppModel RotateCurrentImage(Rotation direction) =>
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
            }).IfNone(this);
    
    /// <summary>
    /// Move to the next file
    /// </summary>
    public AppModel MoveToNextFile()
    {
        return CurrentFile.Match(
            Some: currentId =>
            {
                // Get ordered keys
                var keys = Files.Keys.ToList();
                var index = keys.FindIndex(k => k.Value == currentId.Value);

                if (index >= 0 && index < keys.Count - 1)
                {
                    return this with { CurrentFile = keys[index + 1] };
                }
                return this;
            },
            None: () => this
        );
    }

    /// <summary>
    /// Move to the previous file
    /// </summary>
    public AppModel MoveToPreviousFile() =>
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
            }).IfNone(this);   

    /// <summary>
    /// Update file state
    /// </summary>
    public AppModel UpdateFileState(FileState state) =>
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
            }).IfNone(this);          

    /// <summary>
    /// Update file name
    /// </summary>
    public AppModel UpdateFilename(string newFilename) =>
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
            }).IfNone(this);  
}