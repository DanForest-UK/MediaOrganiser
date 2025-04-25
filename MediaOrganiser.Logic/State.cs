using LanguageExt;
using static LanguageExt.Prelude;
using System.Diagnostics;
using MediaOrganiser.Domain;

namespace MediaOrganiser.Logic;

/// <summary>
/// Manages application state using an immutable and thread safe Atom container
/// </summary>
public static class ObservableState
{
    /// <summary>
    /// Thread safe and atomic management of state
    /// </summary>
    static readonly Atom<AppModel> stateAtom = Atom(new AppModel(
        Files: new Map<FileId, MediaInfo>(),
        WorkInProgress: false,
        CurrentFolder: None,
        CurrentFile: None,
        CopyOnly: new CopyOnly(false),
        SortByYear: new SortByYear(true),
        KeepParentFolder: new KeepParentFolder(false)));

    // Event that fires when state changes
    public static event EventHandler<AppModel>? StateChanged;

    /// <summary>
    /// Current application state (read-only access)
    /// </summary>
    public static AppModel Current => stateAtom.Value;

    /// <summary>
    /// Sets the files in the application state
    /// </summary>
    public static void SetFiles(Seq<MediaInfo> files) =>
        Update(Current.SetFiles(files));

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
    /// Sets the keep parent folder flag
    /// </summary>
    public static void SetKeepParentFolder(bool keepParentFolder) =>
        Update(stateAtom.Value with { KeepParentFolder = new KeepParentFolder(keepParentFolder) });

    /// <summary>
    /// Move to the next file in the collection
    /// </summary>
    public static void NextFile() =>
        Update(Current.MoveToNextFile());

    /// <summary>
    /// Rotates the current image
    /// </summary>
    public static void RotateCurrentImage(Rotation direction) =>
        Update(Current.RotateCurrentImage(direction));

    /// <summary>
    /// Updates the filename of the current file
    /// </summary>
    public static void UpdateFilename(string newFilename) =>
        Update(Current.UpdateFilename(newFilename));

    /// <summary>
    /// Move to the previous file in the collection
    /// </summary>
    public static void PreviousFile() =>
        Update(Current.MoveToPreviousFile());

    /// <summary>
    /// Update file state
    /// </summary>
    public static void UpdateFileState(FileState state) =>
        Update(Current.UpdateFileState(state));

    /// <summary>
    /// Clears all files from the application state
    /// </summary>
    public static void ClearFiles() =>
        Update(stateAtom.Value with
        {
            Files = new Map<FileId, MediaInfo>(),
            CurrentFile = None
        });

    /// <summary>
    /// Updates the entire application state atomically
    /// </summary>
    public static void Update(AppModel newState)
    {
        var oldState = stateAtom.Value;
        stateAtom.Swap(_ => newState);

        if (!oldState.Equals(newState))
        {
            Try.lift(() =>
            {
                StateChanged?.Invoke(null, newState);
            }).IfFail(ex =>
            {
                Debug.WriteLine($"Error in state change notification: {ex.Message}");
                return unit;
            });
        }
    }
}