using LanguageExt;

namespace MediaOrganiser.Domain
{
    public record OrganiseResponse(Seq<UserError> UserErrors, bool Success);
}
