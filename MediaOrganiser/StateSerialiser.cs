using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;
using static SortPhotos.Core.Types;
using static LanguageExt.Prelude;
using System.IO;
using System.Runtime.CompilerServices;
using SortPhotos.Core;
using LanguageExt.Core;

namespace MediaOrganiser
{
    /// <summary>
    /// Handles serialization and deserialization of application state
    /// </summary>
    public static class StateSerializer
    {

        /// <summary>
        /// A serializable version of the application state
        /// </summary>
        public record SerializableAppModel(MediaInfo[] Files, int CurrentFileId, string CurrentFolder, bool CopyOnly, bool SortByYear);

        const string StateFileName = "appstate.json";

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
        };

        /// <summary>
        /// Gets the full path to the state file in the application directory
        /// </summary>
        public static string GetStateFilePath()
        {
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appDirectory, StateFileName);
        }

        /// <summary>
        /// Serializes the current application state to disk
        /// </summary>
        public static void SaveState(AppModel state)
        {
            try
            {
                if (state.Files.Values.Any())
                {
                    // Create a serializable version of the state
                    var serializableState = state.ToSerializableAppModel();
                    var json = JsonSerializer.Serialize(serializableState, SerializerOptions);
                    File.WriteAllText(GetStateFilePath(), json);

                    System.Diagnostics.Debug.WriteLine("State saved successfully");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving state: {ex.Message}");
            }
        }
        

        /// <summary>
        /// Attempts to load saved state from disk
        /// </summary>
        public static Option<AppModel> LoadState()
        {
            try
            {
                var statePath = GetStateFilePath();
                if (!File.Exists(statePath))
                    return None;

                var json = File.ReadAllText(statePath);
                var serializableState = JsonSerializer.Deserialize<SerializableAppModel>(json, SerializerOptions);

                if (serializableState == null)
                    return None;

                // Filter out any file we can't find or have an error trying to find
                var presentFiles = serializableState.Files.Where(f => 
                    Try.lift(() => File.Exists(f.FullPath.Value)) 
                        .IfFail(err => false)).ToArray();

                if (!presentFiles.Any())
                    return None;

                return (serializableState with { Files = presentFiles }).ToAppModel();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading state: {ex.Message}");
                return None;
            }
        }

        public static SerializableAppModel ToSerializableAppModel(this AppModel model) =>
            new SerializableAppModel(
                Files: model.Files.Values.ToArray(),
                CurrentFileId: model.CurrentFile.Map(v => v.Value).IfNone(0),
                CurrentFolder: model.CurrentFolder.Map(v => v.Value).IfNone(""),
                CopyOnly: model.CopyOnly.Value,
                SortByYear: model.SortByYear.Value);

        public static AppModel ToAppModel(this SerializableAppModel model) =>
            new AppModel(
                Files: toMap(model.Files.Select(m => (m.Id, m))),
                WorkInProgress: false,
                CurrentFolder: model.CurrentFolder.HasValue()
                    ? Some(new FolderPath(model.CurrentFolder))
                    : None,
                CurrentFile: model.CurrentFileId == 0
                    ? None
                    : new FileId(model.CurrentFileId),
                CopyOnly: new CopyOnly(model.CopyOnly),
                SortByYear: new SortByYear(model.SortByYear));
      

        /// <summary>
        /// Deletes the saved state file if it exists
        /// </summary>
        public static void DeleteState()
        {
            try
            {
                var statePath = GetStateFilePath();
                if (File.Exists(statePath))
                {
                    File.Delete(statePath);
                    System.Diagnostics.Debug.WriteLine("State file deleted");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting state file: {ex.Message}");
            }
        }
    }  
}