namespace MediaOrganiser.Domain
{
    public enum FileCategory
    {
        Image,
        Video,
        Document,
        Unknown
    }

    public enum FileState
    {
        Keep,
        Bin,
        Undecided
    }

    public enum Rotation
    {
        None = 0,
        Rotate90 = 90,
        Rotate180 = 180,
        Rotate270 = 270
    }
}
