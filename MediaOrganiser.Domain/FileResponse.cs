using LanguageExt;

namespace MediaOrganiser.Domain
{
    public record FileResponse(Seq<UserError> UserErrors, Seq<MediaInfo> Files);
}
