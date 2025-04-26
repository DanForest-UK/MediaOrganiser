using LanguageExt;

namespace MediaOrganiser.Domain;

/// <summary>
/// Response type for organising files
/// </summary>
public record OrganiseResponse(Seq<UserError> UserErrors, bool Success);
