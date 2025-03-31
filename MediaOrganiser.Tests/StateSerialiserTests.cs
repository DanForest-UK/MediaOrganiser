using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediaOrganiser;
using MediaOrganiser.Core;
using static MediaOrganiser.Core.Types;
using LanguageExt;
using static LanguageExt.Prelude;
using System.IO;
using System.Linq;
using System;
using System.Text.Json;

namespace MediaOrganiser.Tests
{
    /// <summary>
    /// Contains unit tests for the StateSerialiser class, which is responsible for 
    /// serializing and deserializing the application state.
    /// </summary>
    [TestClass]
    public class StateSerialiserTests
    {
        private AppModel testModel;

        /// <summary>
        /// Sets up the test environment before each test runs.
        /// Creates a temporary directory for test state files and initializes
        /// a test model with sample data for use in tests.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Create a temporary path for test state file
            StateSerialiser.StateFilePath = Path.Combine(Path.GetTempPath(), "MediaOrganiserTests", Guid.NewGuid().ToString(), "appstate.json");
            Directory.CreateDirectory(Path.GetDirectoryName(StateSerialiser.StateFilePath));

            // Create the test state file to ensure it exists for tests
            TestDataFactory.CreateTestFile(StateSerialiser.StateFilePath, 0);

            // Create a test model that can be used by tests
            var files = toMap(new[]
            {
                (new FileId(1), TestDataFactory.CreateMediaInfo(1, "file1", FileCategory.Image)),
                (new FileId(2), TestDataFactory.CreateMediaInfo(2, "file2", FileCategory.Image)),
                (new FileId(3), TestDataFactory.CreateMediaInfoFromFile(3, StateSerialiser.StateFilePath)) // include at least one valid file
            });

            testModel = new AppModel(
                Files: files,
                WorkInProgress: false,
                CurrentFolder: new FolderPath("C:\\test"),
                CurrentFile: new FileId(1),
                CopyOnly: new CopyOnly(true),
                SortByYear: new SortByYear(false),
                KeepParentFolder: new KeepParentFolder(true));
        }

        /// <summary>
        /// Cleans up the test environment after each test completes.
        /// Removes temporary files and directories created during testing.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test directory
            try
            {
                if (Directory.Exists(Path.GetDirectoryName(StateSerialiser.StateFilePath)))
                {
                    Directory.Delete(Path.GetDirectoryName(StateSerialiser.StateFilePath), true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up test directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests that the ToSerializableAppModel method correctly converts an AppModel 
        /// to a serializable representation.
        /// </summary>
        [TestMethod]
        public void ToSerialisable()
        {
            var serializable = testModel.ToSerializableAppModel();

            Assert.IsNotNull(serializable);
            Assert.AreEqual(3, serializable.Files.Length);
            Assert.AreEqual(1, serializable.CurrentFileId);
            Assert.AreEqual("C:\\test", serializable.CurrentFolder);
            Assert.IsTrue(serializable.CopyOnly);
            Assert.IsFalse(serializable.SortByYear);
            Assert.IsTrue(serializable.KeepParentFolder);
        }

        /// <summary>
        /// Tests that the ToAppModel method correctly converts a serializable representation 
        /// back to an AppModel.
        /// </summary>
        [TestMethod]
        public void ToAppModel()
        {
            // Arrange
            var serializable = new StateSerialiser.SerialisableAppModel(
                Files: new[]
                {
                    TestDataFactory.CreateMediaInfo(1, "file1", FileCategory.Image),
                    TestDataFactory.CreateMediaInfo(2, "file2", FileCategory.Image)
                },
                CurrentFileId: 1,
                CurrentFolder: "C:\\test",
                CopyOnly: true,
                SortByYear: false,
                KeepParentFolder: true);

            // Act
            var model = serializable.ToAppModel();

            // Assert
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Files.Count);
            Assert.AreEqual(1, model.CurrentFile.Match(id => id.Value, () => -1));
            Assert.AreEqual("C:\\test", model.CurrentFolder.Map(f => f.Value).IfNone(""));
            Assert.IsTrue(model.CopyOnly.Value);
            Assert.IsFalse(model.SortByYear.Value);
            Assert.IsTrue(model.KeepParentFolder.Value);
        }

        /// <summary>
        /// Tests that the SaveState method successfully writes the application state to disk.
        /// Verifies that the file is created and contains valid, deserializable content.
        /// </summary>
        [TestMethod]
        public void SaveState()
        {
            // Act
            StateSerialiser.SaveState(testModel);

            // Assert
            Assert.IsTrue(File.Exists(StateSerialiser.StateFilePath));
            var content = File.ReadAllText(StateSerialiser.StateFilePath);
            Assert.IsFalse(string.IsNullOrEmpty(content));

            // Verify we can deserialize the content
            var deserializedState = JsonSerializer.Deserialize<StateSerialiser.SerialisableAppModel>(
                content, new JsonSerializerOptions { WriteIndented = true });
            Assert.IsNotNull(deserializedState);
            Assert.AreEqual(3, deserializedState.Files.Length);
        }

        /// <summary>
        /// Tests that the SaveState method does not create a state file when the model 
        /// contains no files. This prevents empty or unnecessary state files.
        /// </summary>
        [TestMethod]
        public void SaveStateNoFiles()
        {
            // Arrange
            var emptyModel = new AppModel(
                Files: new Map<FileId, MediaInfo>(),
                WorkInProgress: false,
                CurrentFolder: Some(new FolderPath("C:\\test")),
                CurrentFile: None,
                CopyOnly: new CopyOnly(true),
                SortByYear: new SortByYear(false),
                KeepParentFolder: new KeepParentFolder(true));

            // Delete the existing file first to test that no new file is created
            if (File.Exists(StateSerialiser.StateFilePath))
                File.Delete(StateSerialiser.StateFilePath);

            // Act
            StateSerialiser.SaveState(emptyModel);

            // Assert
            Assert.IsFalse(File.Exists(StateSerialiser.StateFilePath));
        }

        /// <summary>
        /// Tests that LoadState returns None when no state file exists.
        /// </summary>
        [TestMethod]
        public void NoStateFileIsNone()
        {
            // Delete any existing file first
            if (File.Exists(StateSerialiser.StateFilePath))
                File.Delete(StateSerialiser.StateFilePath);

            Assert.IsTrue(StateSerialiser.LoadState().IsNone);
        }

        /// <summary>
        /// Tests that LoadState correctly loads a saved state file and returns an AppModel.
        /// Verifies that only files with valid paths are included in the loaded model.
        /// </summary>
        [TestMethod]
        public void LoadState()
        {
            StateSerialiser.SaveState(testModel);

            var result = StateSerialiser.LoadState();

            Assert.IsTrue(result.IsSome);
            result.IfSome(loadedModel =>
            {
                Assert.AreEqual(1, loadedModel.Files.Count); // All should be filtered out except the one with a valid path
                Assert.AreEqual(1, loadedModel.CurrentFile.Map(id => id.Value).IfNone(0));
                Assert.IsTrue(loadedModel.CopyOnly.Value);
                Assert.IsFalse(loadedModel.SortByYear.Value);
                Assert.IsTrue(loadedModel.KeepParentFolder.Value);
            });
        }

        /// <summary>
        /// Tests that LoadState returns None when the state file contains invalid or corrupt JSON.
        /// This ensures the application can gracefully handle corrupted state files.
        /// </summary>
        [TestMethod]
        public void CorruptStateIsNone()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(StateSerialiser.StateFilePath));
            File.WriteAllText(StateSerialiser.StateFilePath, "This is not valid JSON");

            var result = StateSerialiser.LoadState();
            Assert.IsTrue(result.IsNone);
        }

        /// <summary>
        /// Tests that the DeleteState method successfully removes an existing state file.
        /// </summary>
        [TestMethod]
        public void DeleteState()
        {
            StateSerialiser.SaveState(testModel);
            Assert.IsTrue(File.Exists(StateSerialiser.StateFilePath));
            StateSerialiser.DeleteState();
            Assert.IsFalse(File.Exists(StateSerialiser.StateFilePath));
        }

        /// <summary>
        /// Tests that the DeleteState method does not throw exceptions when attempting 
        /// to delete a non-existent state file.
        /// </summary>
        [TestMethod]
        public void DeleteStateWithNoState()
        {
            if (File.Exists(StateSerialiser.StateFilePath))
                File.Delete(StateSerialiser.StateFilePath);

            try
            {
                StateSerialiser.DeleteState();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail("DeleteState should not throw when file doesn't exist");
            }
        }
    }
}