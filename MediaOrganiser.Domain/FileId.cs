namespace MediaOrganiser.Domain;

/// <summary>
/// File ID, key of the map of files, will be sequentially ordered
/// </summary>
public record FileId(int Value) : IComparable<FileId>
{
    /// <summary>
    /// IComparable for sorting in Map type
    /// </summary>
    public int CompareTo(FileId? other) =>
        other is null ? 1 : Value.CompareTo(other.Value);
}
