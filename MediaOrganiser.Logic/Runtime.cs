using LanguageExt;
using MediaOrganiser.Domain;

namespace MediaOrganiser.Logic;

/// <summary>
/// Dependency injection
/// </summary>
public static class Runtime
{
    /// <summary>
    /// Current rotate and save is Windows forms specific, this delegate can be used if the Domain/Logic were
    /// to be used on a different front end
    /// </summary>
    public static Func<MediaInfo, string, IO<Unit>> RotateImageAndSave = (_, _) => throw new NotImplementedException();
}
