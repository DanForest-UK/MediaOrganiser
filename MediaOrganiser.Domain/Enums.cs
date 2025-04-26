namespace MediaOrganiser.Domain;

/// <summary>
/// Type of file
/// </summary>
public enum FileCategory
{
    Image,
    Video,
    Document,
    Unknown
}

/// <summary>
/// What will be done with the file on organise
/// </summary>
public enum FileState
{
    Keep,
    Bin,
    Undecided
}

/// <summary>
/// How rotated the final image should be
/// </summary>
public enum Rotation
{
    None = 0,
    Rotate90 = 90,
    Rotate180 = 180,
    Rotate270 = 270
}
