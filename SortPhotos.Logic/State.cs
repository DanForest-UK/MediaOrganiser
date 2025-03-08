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

namespace MusicTools.Logic
{
    /// <summary>
    /// Manages application state using an immutable Atom container
    /// </summary>
    public static class ObservableState
    {
        // Thread safe and atomic management of state
        static readonly Atom<AppModel> stateAtom = Atom(new AppModel(
            Files: new Map<FileId, MediaInfo>()));

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
            // Ensure unique ordered Ids
            if (files.Select(s => s.Id).Distinct().Length != files.Length)
                files = toSeq(files.Select((s, i) => s with { Id = new FileId(i + 1) }));

            var newState = stateAtom.Value with
            {
                Files = (from f in files
                         select (f.Id, f)).ToMap()
            };

            Update(newState);
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