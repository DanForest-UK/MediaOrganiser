namespace MediaOrganiser.Domain;

/// <summary>
/// Option for whether we reproduce the parent folder of a file when we organise
/// </summary>
public readonly record struct KeepParentFolder(bool Value);
