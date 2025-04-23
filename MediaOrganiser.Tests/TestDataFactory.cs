using MediaOrganiser.Domain;

namespace MediaOrganiser.Tests
{
    /// <summary>
    /// Provides utility methods for creating test data objects
    /// </summary>
    public static class TestDataFactory
    {
        /// <summary>
        /// Creates a MediaInfo record with basic values for testing
        /// </summary>
        /// <param name="id">ID to assign to the media</param>
        /// <param name="filename">Filename without extension</param>
        /// <param name="category">Media category type</param>
        /// <param name="state">State of the file (defaults to Undecided)</param>
        /// <returns>A MediaInfo record with test values</returns>
        public static MediaInfo CreateMediaInfo(
            int id, 
            string filename, 
            FileCategory category, 
            FileState state = FileState.Undecided) =>
            new(
                new FileId(id),
                new FileName(filename),
                new FullPath($"C:\\test\\{filename}.jpg"),
                new Extension(".jpg"),
                new Size(1024),
                new Date(DateTime.Now),
                category,
                state);

        /// <summary>
        /// Creates a MediaInfo record with a specific path
        /// </summary>
        /// <param name="id">ID to assign to the media</param>
        /// <param name="filename">Filename without extension</param>
        /// <param name="category">Media category type</param>
        /// <param name="fullPath">Full path to the file</param>
        /// <param name="extension">File extension including the dot</param>
        /// <param name="state">State of the file (defaults to Undecided)</param>
        /// <returns>A MediaInfo record with the specified values</returns>
        public static MediaInfo CreateMediaInfo(
            int id,
            string filename,
            FileCategory category,
            string fullPath,
            string extension,
            FileState state = FileState.Undecided) =>
            new(
                new FileId(id),
                new FileName(filename),
                new FullPath(fullPath),
                new Extension(extension),
                new Size(1024),
                new Date(DateTime.Now),
                category,
                state);

        /// <summary>
        /// Creates a MediaInfo record from an existing file path
        /// </summary>
        /// <param name="id">ID to assign to the media</param>
        /// <param name="fullPath">Path to an existing file</param>
        /// <param name="state">State of the file (defaults to Undecided)</param>
        /// <returns>A MediaInfo record with metadata from the actual file</returns>
        public static MediaInfo CreateMediaInfoFromFile(
            int id,
            string fullPath,
            FileState state = FileState.Undecided)
        {
            var fileInfo = new FileInfo(fullPath);
            var filename = Path.GetFileNameWithoutExtension(fullPath);
            var extension = Path.GetExtension(fullPath);
            var category = GetCategoryFromExtension(extension);
            
            return new MediaInfo(
                new FileId(id),
                new FileName(filename),
                new FullPath(fullPath),
                new Extension(extension),
                new Size(fileInfo.Exists ? fileInfo.Length : 0),
                new Date(fileInfo.Exists ? fileInfo.LastWriteTime : DateTime.Now),
                category,
                state);
        }

        /// <summary>
        /// Determines file category based on file extension
        /// </summary>
        private static FileCategory GetCategoryFromExtension(string extension)
        {
            var ext = extension.ToLowerInvariant();
            
            if (ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".tiff" or ".webp")
                return FileCategory.Image;
                
            if (ext is ".mp4" or ".avi" or ".mov" or ".wmv" or ".mkv" or ".flv" or ".webm")
                return FileCategory.Video;
                
            if (ext is ".pdf" or ".docx" or ".doc" or ".txt" or ".rtf")
                return FileCategory.Document;
                
            return FileCategory.Unknown;
        }

        /// <summary>
        /// Creates a test file with the specified size
        /// </summary>
        /// <param name="path">Path where the file will be created</param>
        /// <param name="sizeInBytes">Size of the file in bytes</param>
        public static void CreateTestFile(string path, int sizeInBytes)
        {
            var directory = Path.GetDirectoryName(path);
            if (directory != null)
                Directory.CreateDirectory(directory);

            using var fs = File.Create(path);
            fs.SetLength(sizeInBytes);
            
            // Set a realistic timestamp
            File.SetLastWriteTime(path, DateTime.Now.AddDays(-new Random().Next(1, 30)));
        }
    }
}
