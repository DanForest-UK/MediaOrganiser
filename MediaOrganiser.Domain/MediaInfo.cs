namespace MediaOrganiser.Domain
{
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
}
