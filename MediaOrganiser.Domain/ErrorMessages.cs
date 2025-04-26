namespace MediaOrganiser.Domain;

/// <summary>
/// Error message descriptions
/// </summary>
public static class ErrorMessages
{
    public const string ThereWasAProblem = "There was a problem";
    public const string FileSystemAccessNeeded = "File access needs to be granted for this app in Privacy & Security -> File system";
    public const string AccessToPathDeniedPrefix = "Access to:";
    public const string ErrorGettingFilesPrefix = "Error getting files type:";
    public const string UnauthorisedAccessPrefix = "You do not have sufficient privalages for:";
    public const string FileNotFoundPrefix = "File not found:";
    public const string DirectoryNotFoundPrefix = "Directory not found:";
    public const string ErrorReadingFilePrefix = "Error reading file:";
    public const string UnableToMovePrefix = "Unable to move/copy file:";
    public const string UnableToDeletePrefix = "Unable to delete file";
    public const string UnableToRotateSuffix = ", it was copied in its original orientation";
    public const string UnableToRotatePrefix = "Unable to rotate file";
    public const string UnableToCreateDirectoryPrefix = "Unable to create directory";
    public const string PathIsEmpty = "Path is empty";
    public const string DirectoryInvalidPrefix = "Directory is invalid:";
}
