using LanguageExt;

namespace MediaOrganiser.Domain;

/// <summary>
/// Result type for scanning a directory of files
/// </summary>
public record FileResponse(Seq<UserError> UserErrors, Seq<MediaInfo> Files);
