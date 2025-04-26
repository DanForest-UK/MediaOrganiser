using System.Text.Json;
using LanguageExt;
using static LanguageExt.Prelude;
using System.Diagnostics;
using MediaOrganiser.Domain;

namespace MediaOrganiser.Logic;

/// <summary>
/// Handles serialization and deserialization of application state
/// </summary>
public static class StateSerialiser
{
    public static string StateFilePath = "";
    
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

    /// <summary>
    /// Options for JSON serialization,  makes it easier to read by eye
    /// </summary>
    static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };
         
    /// <summary>
    /// Serializes the current application state to disk
    /// </summary>
    public static Unit SaveState(AppModel state) =>
        Try.lift(() =>
        {
            if (state.Files.Values.Any())
            {
                var serializableState = state.ToSerializableAppModel();
                var json = JsonSerializer.Serialize(serializableState, SerializerOptions);
                File.WriteAllText(StateFilePath, json);
            }
            return unit;
        }).IfFail(ex =>
        {
            Debug.WriteLine($"Error saving state: {ex.Message}");
            return unit;
        });

    /// <summary>
    /// Attempts to load saved state from disk
    /// </summary>
    public static Option<AppModel> LoadState() =>
        Try.lift(() =>
        {
            if (!File.Exists(StateFilePath))
                return None;

            return Optional(JsonSerializer.Deserialize<SerialisableAppModel>(File.ReadAllText(StateFilePath), SerializerOptions))
                .Map(state =>
                {
                    // Filter out any file we can't find or have an error trying to find
                    var presentFiles = state.Files.Where(f =>
                        Try.lift(() => File.Exists(f.FullPath.Value))
                            .IfFail(err => false))
                            .Select((s, i) => s with { Id = new FileId(i + 1) })
                            .ToArray(); // reset index of files

                    // If any files are missing since last session we start at the beginning again
                    var currentFile = presentFiles.Length == state.Files.Length &&
                                      state.CurrentFileId < presentFiles.Length
                        ? state.CurrentFileId
                        : 1;

                    return (state with { Files = presentFiles, CurrentFileId = currentFile }).ToAppModel();
                })
                .Where(s => s.Files.Values.Any());
        }).IfFail(ex =>
        {
            Debug.WriteLine($"Error loading state: {ex.Message}");
            return None;
        });

    /// <summary>
    /// Converts app model to something that serializes nicely
    /// </summary>
    public static SerialisableAppModel ToSerializableAppModel(this AppModel model) =>
        new(
            Files: [.. model.Files.Values],
            CurrentFileId: model.CurrentFile.Map(v => v.Value).IfNone(0),
            CurrentFolder: model.CurrentFolder.Map(v => v.Value).IfNone(""),
            CopyOnly: model.CopyOnly.Value,
            SortByYear: model.SortByYear.Value,
            KeepParentFolder: model.KeepParentFolder.Value);

    /// <summary>
    /// Converts serializable version back to main app model
    /// </summary>
    public static AppModel ToAppModel(this SerialisableAppModel model) =>
        new(
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
            var statePath = StateFilePath;
            if (File.Exists(statePath))
                File.Delete(statePath);
            return unit;
        }).IfFail(ex =>
        {
            Debug.WriteLine($"Error deleting state file: {ex.Message}");
            return unit;
        });
}