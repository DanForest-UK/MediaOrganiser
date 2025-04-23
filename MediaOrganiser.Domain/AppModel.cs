using static LanguageExt.Prelude;
using LanguageExt;

namespace MediaOrganiser.Domain
{
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

            // Create file map
            var fileMap = (from f in files
                           select (f.Id, f)).ToMap();

            // Create initial state with files
            var newState = this with
            {
                Files = fileMap,
                CurrentFile = files.Length > 0
                    ? files.First().Id
                    : Option<FileId>.None
            };

            // Set current file to the first image if available
            var firstImage = files.FirstOrDefault(f => f.Category == FileCategory.Image);
            if (firstImage != null && newState.CurrentFile.IsNone)
                newState = newState with { CurrentFile = Option<FileId>.Some(firstImage.Id) };

            return newState;
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

                        // Calculate new rotation
                        Rotation newRotation = file.Rotation;
                        if (direction == Rotation.Rotate90)
                        {
                            newRotation = (Rotation)(((int)file.Rotation + 90) % 360);
                        }
                        else if (direction == Rotation.Rotate270)
                        {
                            newRotation = (Rotation)(((int)file.Rotation + 270) % 360);
                        }

                        var updatedFile = file with { Rotation = newRotation };

                        // Return updated state
                        return this with
                        {
                            Files = Files.AddOrUpdate(currentId, updatedFile)
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
                    var keys = Files.Keys.OrderBy(k => k.Value).ToList();
                    var index = keys.FindIndex(k => k.Value == currentId.Value);

                    if (index >= 0 && index < keys.Count - 1)
                    {
                        return this with { CurrentFile = Option<FileId>.Some(keys[index + 1]) };
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
                    // Get ordered keys
                    var keys = Files.Keys.OrderBy(k => k.Value).ToList();
                    var index = keys.FindIndex(k => k.Value == currentId.Value);

                    if (index > 0)
                    {
                        return this with { CurrentFile = Option<FileId>.Some(keys[index - 1]) };
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
                        var file = Files[currentId];
                        var updatedFile = file with { State = state };

                        return this with
                        {
                            Files = Files.AddOrUpdate(currentId, updatedFile)
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
                        var file = Files[currentId];
                        var updatedFile = file with { FileName = new FileName(newFilename) };

                        return this with
                        {
                            Files = Files.AddOrUpdate(currentId, updatedFile)
                        };
                    }
                    return this;
                }).IfNone(this);          
        
    }
}