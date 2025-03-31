using LanguageExt;
using System;
using System.Linq;
using static LanguageExt.Prelude;
using static MediaOrganiser.Core.Types;

namespace MediaOrganiser.Core
{
    public static class Logic
    {
        public static int CountFilesForDeletion(this AppModel model) =>
           model.Files.Values.Count(f => f.State == FileState.Bin);

        /// <summary>
        /// Add new set of files into an app model
        /// </summary>
        public static AppModel SetFiles(this AppModel model, Seq<MediaInfo> files)
        {
            // Filter and assign IDs
            files = toSeq(files.Where(s => s.Category != FileCategory.Unknown)
                              .Select((s, i) => s with { Id = new FileId(i + 1) }));

            // Create file map
            var fileMap = (from f in files
                           select (f.Id, f)).ToMap();

            // Create initial state with files
            var newState = model with
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
        public static AppModel RotateCurrentImage(this AppModel model, Rotation direction)
        {
            return model.CurrentFile.Match(
                Some: currentId =>
                {
                    if (model.Files.ContainsKey(currentId))
                    {
                        var file = model.Files[currentId];

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
                        return model with
                        {
                            Files = model.Files.AddOrUpdate(currentId, updatedFile)
                        };
                    }
                    return model;
                },
                None: () => model
            );
        }

        /// <summary>
        /// Move to the next file
        /// </summary>
        public static AppModel MoveToNextFile(this AppModel model)
        {
            return model.CurrentFile.Match(
                Some: currentId =>
                {
                    // Get ordered keys
                    var keys = model.Files.Keys.OrderBy(k => k.Value).ToList();
                    var index = keys.FindIndex(k => k.Value == currentId.Value);

                    if (index >= 0 && index < keys.Count - 1)
                    {
                        return model with { CurrentFile = Option<FileId>.Some(keys[index + 1]) };
                    }
                    return model;
                },
                None: () => model
            );
        }

        /// <summary>
        /// Move to the previous file
        /// </summary>
        public static AppModel MoveToPreviousFile(this AppModel model)
        {
            return model.CurrentFile.Match(
                Some: currentId =>
                {
                    // Get ordered keys
                    var keys = model.Files.Keys.OrderBy(k => k.Value).ToList();
                    var index = keys.FindIndex(k => k.Value == currentId.Value);

                    if (index > 0)
                    {
                        return model with { CurrentFile = Option<FileId>.Some(keys[index - 1]) };
                    }
                    return model;
                },
                None: () => model
            );
        }

        /// <summary>
        /// Update file state
        /// </summary>
        public static AppModel UpdateFileState(this AppModel model, FileState state)
        {
            return model.CurrentFile.Match(
                Some: currentId =>
                {
                    if (model.Files.ContainsKey(currentId))
                    {
                        var file = model.Files[currentId];
                        var updatedFile = file with { State = state };

                        return model with
                        {
                            Files = model.Files.AddOrUpdate(currentId, updatedFile)
                        };
                    }
                    return model;
                },
                None: () => model
            );
        }

        /// <summary>
        /// Update file name
        /// </summary>
        public static AppModel UpdateFilename(this AppModel model, string newFilename)
        {
            return model.CurrentFile.Match(
                Some: currentId =>
                {
                    if (model.Files.ContainsKey(currentId))
                    {
                        var file = model.Files[currentId];
                        var updatedFile = file with { FileName = new FileName(newFilename) };

                        return model with
                        {
                            Files = model.Files.AddOrUpdate(currentId, updatedFile)
                        };
                    }
                    return model;
                },
                None: () => model
            );
        }
    }
}