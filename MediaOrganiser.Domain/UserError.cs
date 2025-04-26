namespace MediaOrganiser.Domain;

/// <summary>
/// User expected error class, to distinguish from unexpected exception messages
/// </summary>
public record UserError(string Message)
{
    public const int DisplayErrorCode = 303;
}
