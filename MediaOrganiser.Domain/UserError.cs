namespace MediaOrganiser.Domain
{
    public record UserError(string Message)
    {
        public const int DisplayErrorCode = 303;
    }
}
