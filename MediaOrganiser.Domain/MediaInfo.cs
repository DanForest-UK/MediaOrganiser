namespace MediaOrganiser.Domain;

/// <summary>
/// Type for information about the file scanned
/// </summary>
/// <param name="Id">Arbitrary ID just needs to be unique in the app and sequentially ordered</param>
/// <param name="FileName">File name without extension</param>
/// <param name="FullPath">Full path to file</param>
/// <param name="Extension">File extension</param>
/// <param name="Size">File size in bytes</param>
/// <param name="Date">Date of file</param>
/// <param name="Category">What type of media the file is</param>
/// <param name="State">Decision as to whether file is kept or binned</param>
/// <param name="Rotation">How much an image should be rotated on organise</param>
public record MediaInfo(
    FileId Id,
    FileName FileName,
    FullPath FullPath,
    Extension Extension,
    Size Size,
    Date Date,
    FileCategory Category,
    FileState State,
    Rotation Rotation = Rotation.None);
