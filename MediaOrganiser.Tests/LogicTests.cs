using Microsoft.VisualStudio.TestTools.UnitTesting;
using LanguageExt;
using static LanguageExt.Prelude;
using MediaOrganiser.Domain;

namespace MediaOrganiser.Tests.Core
{
    [TestClass]
    public class LogicTests
    {
        // All your existing test methods...

        /// <summary>
        /// Set files should keep settings but create new set of files
        /// </summary>

        [TestMethod]
        public void SetFiles()
        {
            // This should get cleared by new set of files
            var existingFile = CreateMediaInfo(42, "existing", FileCategory.Document) with { State = FileState.Keep };

            var existingModel = new AppModel(
                new Map<FileId, MediaInfo>().Add(existingFile.Id, existingFile),
                true, 
                new FolderPath("C:\\output"), 
                existingFile.Id,
                new CopyOnly(true),
                new SortByYear(false),
                new KeepParentFolder(true));

            var newFiles = toSeq(new[]
            {
                CreateMediaInfo(0, "new1", FileCategory.Image),
                CreateMediaInfo(0, "new2", FileCategory.Video)
            });
                 
            var result = existingModel.SetFiles(newFiles);

            // Should have added the new files (with IDs 1 and 2)
            Assert.AreEqual(2, result.Files.Count);
            Assert.IsTrue(result.Files.ContainsKey(new FileId(1)));
            Assert.IsTrue(result.Files.ContainsKey(new FileId(2)));

            // Should remove existing file
            Assert.IsTrue(!result.Files.ContainsKey(new FileId(42)));
           
            // Should preserve settings   
            Assert.AreEqual(true, result.CopyOnly.Value);
            Assert.AreEqual(false, result.SortByYear.Value);
            Assert.AreEqual(true, result.KeepParentFolder.Value);

            // Current file should be first file
            Assert.IsTrue(result.CurrentFile.Map(v => v.Value) == 1);
        }

        private static MediaInfo CreateMediaInfo(int id, string filename, FileCategory category)
        {
            return new MediaInfo(
                new FileId(id),
                new FileName(filename),
                new FullPath($"C:\\test\\{filename}.jpg"),
                new Extension(".jpg"),
                new Size(1024),
                new Date(DateTime.Now),
                category,
                FileState.Undecided);
        }       
    }
}
