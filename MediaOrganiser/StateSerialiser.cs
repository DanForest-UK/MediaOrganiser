using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;
using static MediaOrganiser.Core.Types;
using static LanguageExt.Prelude;
using System.IO;
using System.Runtime.CompilerServices;
using MediaOrganiser.Core;
using LanguageExt.Core;
using System.Diagnostics;

namespace MediaOrganiser
{
    /// <summary>
    /// Handles serialization and deserialization of application state
    /// </summary>
    public static class StateSerialiser
    {

        /// <summary>
        /// A serializable version of the application state
        /// </summary>
        public record SerialisableAppModel(
            MediaInfo[] Files,
            int CurrentFileId,
            string CurrentFolder,
            bool CopyOnly,
            bool SortByYear,
            bool KeepParentFolder);

        const string StateFileName = "appstate.json";

        static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
        };

        /// <summary>
        /// Gets the full path to the state file in the application directory
        /// </summary>
        public static string GetStateFilePath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, StateFileName);
       
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
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving state: {ex.Message}");
            }
        }


        /// <summary>
        /// Attempts to load saved state from disk
        /// </summary>
        public static Option<AppModel> LoadState() =>
            Try.lift(() =>
            {
                var statePath = GetStateFilePath();
                if (!File.Exists(statePath))
                    return None;

                return Optional(JsonSerializer.Deserialize<SerialisableAppModel>(File.ReadAllText(statePath), SerializerOptions))
                    .Map(state =>
                    {
                        // Filter out any file we can't find or have an error trying to find
                        var presentFiles = state.Files.Where(f =>
                            Try.lift(() => File.Exists(f.FullPath.Value))
                                .IfFail(err => false)).ToArray();

                        return (state with { Files = presentFiles }).ToAppModel();
                    })
                    .Where(s => s.Files.Values.Any());
            }).IfFail(ex =>
            {
                Debug.WriteLine($"Error loading state: {ex.Message}");
                return None;
            });       

        public static SerialisableAppModel ToSerializableAppModel(this AppModel model) =>
            new SerialisableAppModel(
                Files: model.Files.Values.ToArray(),
                CurrentFileId: model.CurrentFile.Map(v => v.Value).IfNone(0),
                CurrentFolder: model.CurrentFolder.Map(v => v.Value).IfNone(""),
                CopyOnly: model.CopyOnly.Value,
                SortByYear: model.SortByYear.Value,
                KeepParentFolder: model.KeepParentFolder.Value);

        public static AppModel ToAppModel(this SerialisableAppModel model) =>
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
                SortByYear: new SortByYear(model.SortByYear),
                KeepParentFolder: new KeepParentFolder(model.KeepParentFolder));


        /// <summary>
        /// Deletes the saved state file if it exists
        /// </summary>
        public static void DeleteState() =>
            Try.lift(() =>
            {
                var statePath = GetStateFilePath();
                if (File.Exists(statePath))                
                    File.Delete(statePath);
                
            }).IfFail(ex =>
            {
                Debug.WriteLine($"Error deleting state file: {ex.Message}");
                return unit;
            });
    }
}