using LanguageExt;
using System;
using System.Linq;
using static LanguageExt.Prelude;
using G = System.Collections.Generic;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using static SortPhotos.Core.Types;
using SortPhotos.Core;
using LanguageExt.Core;

namespace MusicTools.Logic
{
    /// <summary>
    /// Manages application state using an immutable Atom container
    /// </summary>
    public static class ObservableState
    {
        // Thread safe and atomic management of state
        static readonly Atom<AppModel> stateAtom = Atom(new AppModel(
            Files: new Map<FileId, MediaInfo>(),
            WorkInProgress: false,
            CurrentFolder: None,
            CurrentFile: None,
            CopyOnly: new CopyOnly(false),
            SortByYear: new SortByYear(true)));

        // Event that fires when state changes
        public static event EventHandler<AppModel>? StateChanged;

        /// <summary>
        /// Current application state (read-only access)
        /// </summary>
        public static AppModel Current => stateAtom.Value;

        /// <summary>
        /// Sets the files in the application state
        /// </summary>
        public static void SetFiles(Seq<MediaInfo> files)
        {
            // Set file ID by order
            files = toSeq(files.Where(s => s.Category != FileCategory.Unknown).Select((s, i) => s with { Id = new FileId(i + 1) }));

            var fileMap = (from f in files
                           select (f.Id, f)).ToMap();

            var newState = stateAtom.Value with
            {
                Files = fileMap,
                CurrentFile = files.Length > 0
                    ? files.First().Id
                    : None
            };

            // Set current file to the first image if there are any
            var firstImage = files.FirstOrDefault(f => f.Category == FileCategory.Image);
            if (firstImage != null && newState.CurrentFile.IsNone)
                newState = newState with { CurrentFile = Option<FileId>.Some(firstImage.Id) };

            Update(newState);
        }

        /// <summary>
        /// Sets the work in progress state
        /// </summary>
        public static void SetWorkInProgress(bool isInProgress) =>
            Update(stateAtom.Value with { WorkInProgress = isInProgress });

        /// <summary>
        /// Sets the current folder path
        /// </summary>
        public static void SetCurrentFolder(string? path) =>
            Update(stateAtom.Value with { CurrentFolder = Optional(path).Map(p => new FolderPath(p)) });

        /// <summary>
        /// Sets the copy only flag
        /// </summary>
        public static void SetCopyOnly(bool copyOnly) =>
            Update(stateAtom.Value with { CopyOnly = new CopyOnly(copyOnly) });

        /// <summary>
        /// Sets the sort by year flag
        /// </summary>
        public static void SetSortByYear(bool sortByYear) =>
            Update(stateAtom.Value with { SortByYear = new SortByYear(sortByYear) });

        /// <summary>
        /// Sets the current file to display
        /// </summary>
        public static void SetCurrentFile(FileId fileId) =>
            Update(stateAtom.Value with { CurrentFile = Some(fileId)});
          

        /// <summary>
        /// Move to the next file in the collection
        /// </summary>
        public static void NextFile()
        {
            var current = stateAtom.Value;
            current.CurrentFile.IfSome(currentId =>
            {
                // Get ordered keys
                var keys = current.Files.Keys.OrderBy(k => k.Value).ToList();
                var index = keys.FindIndex(k => k.Value == currentId.Value);

                if (index >= 0 && index < keys.Count - 1)
                    SetCurrentFile(keys[index + 1]);
            });
        }


        /// <summary>
        /// Move to the previous file in the collection
        /// </summary>
        public static void PreviousFile()
        {
            var current = stateAtom.Value;
            current.CurrentFile.IfSome(currentId =>
            {
                // Get ordered keys
                var keys = current.Files.Keys.OrderBy(k => k.Value).ToList();
                var index = keys.FindIndex(k => k.Value == currentId.Value);

                if (index > 0)
                    SetCurrentFile(keys[index - 1]);
            });
        }

        /// <summary>
        /// Update file state and optionally move to next file
        /// </summary>
        public static void UpdateFileState(FileState state, bool moveToNext = true)
        {
            var current = stateAtom.Value;
            current.CurrentFile.IfSome(currentId =>
            {
                // Get the file and update its state
                if (current.Files.ContainsKey(currentId))
                {
                    var file = current.Files[currentId];
                    var updatedFile = file with { State = state };

                    // Update the state atomically
                    Update(current with
                    {
                        Files = current.Files.AddOrUpdate(currentId, updatedFile)
                    });

                    // Move to next file if requested
                    if (moveToNext)
                        NextFile();
                }
            });
        }

        /// <summary>
        /// Clears all files from the application state
        /// </summary>
        public static void ClearFiles()
        {
            Update(stateAtom.Value with
            {
                Files = new Map<FileId, MediaInfo>(),
                CurrentFile = None
            });
        }

        /// <summary>
        /// Updates the entire application state atomically
        /// </summary>
        public static void Update(AppModel newState)
        {
            var oldState = stateAtom.Value;
            stateAtom.Swap(_ => newState);

            if (!oldState.Equals(newState))
            {
                try
                {
                    StateChanged?.Invoke(null, newState);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in state change notification: {ex.Message}");
                }
            }
        }
    }
}