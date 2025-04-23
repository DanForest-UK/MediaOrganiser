namespace MediaOrganiser.Domain
{
    public record FileId(int Value) : IComparable<FileId>
    {
        // Implement IComparable for sorting in Map type
        public int CompareTo(FileId? other) =>
            other is null ? 1 : Value.CompareTo(other.Value);
    }
}
