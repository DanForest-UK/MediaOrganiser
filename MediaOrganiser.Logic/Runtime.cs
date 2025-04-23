using LanguageExt;
using MediaOrganiser.Domain;

namespace MediaOrganiser.Logic
{
    /// <summary>
    /// Dependency injection
    /// </summary>
    public static class Runtime
    {
        public static Func<MediaInfo, string, IO<Unit>> RotateImageAndSave = (_, _) => throw new NotImplementedException();
    }
}
