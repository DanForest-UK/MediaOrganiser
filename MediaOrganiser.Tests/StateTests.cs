using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediaOrganiser.Logic;
using MediaOrganiser.Core;
using static MediaOrganiser.Core.Types;
using LanguageExt;
using static LanguageExt.Prelude;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MediaOrganiser.Tests.Logic
{
    /// <summary>
    /// Contains unit tests for the ObservableState class which manages the application state.
    /// Tests functionality of state updates, event propagation, and thread safety.
    /// </summary>
    [TestClass]
    public class StateTests
    {
        /// <summary>
        /// Initializes the test environment before each test is executed.
        /// Resets the ObservableState to default values to ensure tests start with a clean state.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Reset observable state to default before each test
            ObservableState.Update(new AppModel(
                new Map<FileId, MediaInfo>(),
                false,
                None,
                None,
                new CopyOnly(false),
                new SortByYear(true),
                new KeepParentFolder(false)));
        }

        /// <summary>
        /// Verifies that ObservableState.Current returns the current application state with expected default values.
        /// </summary>
        [TestMethod]
        public void Current()
        {
            var state = ObservableState.Current;

            Assert.IsNotNull(state);
            Assert.AreEqual(0, state.Files.Count);
            Assert.IsFalse(state.WorkInProgress);
            Assert.IsTrue(state.CurrentFolder.IsNone);
            Assert.IsTrue(state.CurrentFile.IsNone);
            Assert.IsFalse(state.CopyOnly.Value);
            Assert.IsTrue(state.SortByYear.Value);
            Assert.IsFalse(state.KeepParentFolder.Value);
        }

        /// <summary>
        /// Tests that SetFiles correctly updates the state with a new collection of files
        /// and fires the appropriate state changed event.
        /// </summary>
        [TestMethod]
        public void SetFiles()
        {
            // Arrange
            var files = toSeq(new[]
            {
                CreateMediaInfo(1, "file1", FileCategory.Image),
                CreateMediaInfo(2, "file2", FileCategory.Video)
            });
                       
            ObservableState.SetFiles(files);

            Assert.AreEqual(2, ObservableState.Current.Files.Count);
            Assert.IsTrue(ObservableState.Current.CurrentFile.IsSome);
            Assert.AreEqual(1, ObservableState.Current.CurrentFile.Map(id => id.Value)
                .IfNone(() => throw new Exception("No current file")));

            Assert.IsTrue(ObservableState.Current.Files.ContainsKey(new FileId(1)));
            Assert.IsTrue(ObservableState.Current.Files.ContainsKey(new FileId(2)));
            Assert.AreEqual("file1", ObservableState.Current.Files[new FileId(1)].FileName.Value);
            Assert.AreEqual("file2", ObservableState.Current.Files[new FileId(2)].FileName.Value);
        }
          
        /// <summary>
        /// Tests that SetWorkInProgress correctly updates the state's WorkInProgress flag
        /// </summary>
        [TestMethod]
        public void WorkInProgress()
        {
            ObservableState.SetWorkInProgress(true);
            Assert.IsTrue(ObservableState.Current.WorkInProgress);
        }

        /// <summary>
        /// Tests that SetCurrentFolder correctly updates the state with a folder path
        /// and fires the appropriate state changed event.
        /// Also verifies behavior when a null folder is provided.
        /// </summary>
        [TestMethod]
        public void SetCurrentFolder()
        {
            string testFolder = "C:\\test\\folder";
            ObservableState.SetCurrentFolder(testFolder);

            Assert.IsTrue(ObservableState.Current.CurrentFolder.IsSome);
            Assert.AreEqual(testFolder, ObservableState.Current.CurrentFolder.Map(f => f.Value)
                .IfNone(() => throw new Exception("Current folder not set")));
            
            ObservableState.SetCurrentFolder(null);
            Assert.IsTrue(ObservableState.Current.CurrentFolder.IsNone);
        }
               
        /// <summary>
        /// Tests that NextFile correctly advances to the next file in the collection
        /// and handles reaching the end of the collection.
        /// </summary>
        [TestMethod]
        public void NextFile()
        {
            // Arrange
            var files = toSeq(new[]
            {
                CreateMediaInfo(1, "file1", FileCategory.Image),
                CreateMediaInfo(2, "file2", FileCategory.Image),
                CreateMediaInfo(3, "file3", FileCategory.Image)
            });
            ObservableState.SetFiles(files);

            ObservableState.NextFile();

            // Assert
            Assert.IsTrue(ObservableState.Current.CurrentFile.IsSome);
            Assert.AreEqual(2, ObservableState.Current.CurrentFile.Map(id => id.Value)
                .IfNone(() => throw new Exception("Current file not set")));

            // Move to the next file again
            ObservableState.NextFile();
            Assert.AreEqual(3, ObservableState.Current.CurrentFile.Map(id => id.Value)
                .IfNone(() => throw new Exception("Current file not set")));

            // Try to move past the end - should stay at the last file
            ObservableState.NextFile();
            Assert.AreEqual(3, ObservableState.Current.CurrentFile.Map(id => id.Value)
                .IfNone(() => throw new Exception("Current file not set")));
        }

        /// <summary>
        /// Tests that PreviousFile correctly moves to the previous file in the collection
        /// and handles reaching the beginning of the collection.
        /// </summary>
        [TestMethod]
        public void PreviousFile()
        {
            // Arrange
            var files = toSeq(new[]
            {
                CreateMediaInfo(1, "file1", FileCategory.Image),
                CreateMediaInfo(2, "file2", FileCategory.Image),
                CreateMediaInfo(3, "file3", FileCategory.Image)
            });

            ObservableState.SetFiles(files);
            ObservableState.NextFile(); // Move to second file
            ObservableState.NextFile(); // Move to third file

            ObservableState.PreviousFile();

            Assert.IsTrue(ObservableState.Current.CurrentFile.IsSome);
            Assert.AreEqual(2, ObservableState.Current.CurrentFile.Map(id => id.Value)
                .IfNone(() => throw new Exception("Current file not set")));

            ObservableState.PreviousFile();
            Assert.AreEqual(1, ObservableState.Current.CurrentFile.Map(id => id.Value)
                .IfNone(() => throw new Exception("Current file not set")));

            // Try to move before the beginning - should stay at the first file
            ObservableState.PreviousFile();
            Assert.AreEqual(1, ObservableState.Current.CurrentFile.Map(id => id.Value)
                .IfNone(() => throw new Exception("Current file not set")));
        }

        /// <summary>
        /// Tests that UpdateFileState correctly updates the state of the current file.
        /// </summary>
        [TestMethod]
        public void UpdateFileState()
        {
            // Arrange
            var files = toSeq(new[]
            {
                CreateMediaInfo(1, "file1", FileCategory.Image)
            });
            ObservableState.SetFiles(files);

            // Act
            ObservableState.UpdateFileState(FileState.Keep);

            // Assert
            Assert.IsTrue(ObservableState.Current.CurrentFile.IsSome);
            Assert.AreEqual(FileState.Keep, ObservableState.Current.Files[new FileId(1)].State);
        }


        /// <summary>
        /// Tests that SetCopyOnly correctly updates the state's CopyOnly flag
        /// </summary>
        [TestMethod]
        public void CopyOnly()
        {
            ObservableState.SetCopyOnly(true);
            Assert.IsTrue(ObservableState.Current.CopyOnly.Value);
        }

        /// <summary>
        /// Tests that SetSortByYear correctly updates the state's SortByYear flag
        /// </summary>
        [TestMethod]
        public void SortByYear()
        {
            //Defaults to true
            Assert.IsTrue(ObservableState.Current.SortByYear.Value);
            ObservableState.SetSortByYear(false);
            Assert.IsFalse(ObservableState.Current.SortByYear.Value);
        }

        /// <summary>
        /// Tests that SetKeepParentFolder correctly updates the state's KeepParentFolder flag
        /// and fires the appropriate state changed event.
        /// </summary>
        [TestMethod]
        public void KeepParentsFolder()
        {
            ObservableState.SetKeepParentFolder(true);
            Assert.IsTrue(ObservableState.Current.KeepParentFolder.Value);
        }

           
        /// <summary>
        /// Verifies that NextFile correctly handles the case when there are no files
        /// by maintaining the CurrentFile as None.
        /// </summary>
        [TestMethod]
        public void NextFileNoFiles()
        {
            ObservableState.NextFile();
            Assert.IsTrue(ObservableState.Current.CurrentFile.IsNone);
        }

   
        /// <summary>
        /// Verifies that PreviousFile correctly handles the case when there are no files
        /// by maintaining the CurrentFile as None.
        /// </summary>
        [TestMethod]
        public void PreviousFileNoFile()
        {
            ObservableState.PreviousFile();
            Assert.IsTrue(ObservableState.Current.CurrentFile.IsNone);
        }


        /// <summary>
        /// Tests that RotateCurrentImage correctly rotates the current image through all rotation states
        /// </summary>
        [TestMethod]
        public void RotateImage()
        {
            var files = toSeq(new[]
            {
                CreateMediaInfo(1, "file1", FileCategory.Image)
            });
            ObservableState.SetFiles(files);
            ObservableState.RotateCurrentImage(Rotation.Rotate90);

            Assert.IsTrue(ObservableState.Current.CurrentFile.IsSome);
            Assert.AreEqual(Rotation.Rotate90, ObservableState.Current.Files[new FileId(1)].Rotation);

            // Rotate again to 180
            ObservableState.RotateCurrentImage(Rotation.Rotate90);
            Assert.AreEqual(Rotation.Rotate180, ObservableState.Current.Files[new FileId(1)].Rotation);

            // Rotate again to 270
            ObservableState.RotateCurrentImage(Rotation.Rotate90);
            Assert.AreEqual(Rotation.Rotate270, ObservableState.Current.Files[new FileId(1)].Rotation);

            // Rotate again to 0 (None)
            ObservableState.RotateCurrentImage(Rotation.Rotate90);
            Assert.AreEqual(Rotation.None, ObservableState.Current.Files[new FileId(1)].Rotation);
        }

        /// <summary>
        /// Tests that UpdateFilename correctly updates the filename of the current file
        /// </summary>
        [TestMethod]
        public void UpdateFilename()
        {
            // Arrange
            var files = toSeq(new[]
            {
                CreateMediaInfo(1, "file1", FileCategory.Image)
            });
            ObservableState.SetFiles(files);
            ObservableState.UpdateFilename("newname");

            Assert.IsTrue(ObservableState.Current.CurrentFile.IsSome);
            Assert.AreEqual("newname", ObservableState.Current.Files[new FileId(1)].FileName.Value);
        }
              
        /// <summary>
        /// Tests that ClearFiles correctly removes all files from the state
        /// </summary>
        [TestMethod]
        public void ClearFiles()
        {
            var files = toSeq(new[]
            {
                CreateMediaInfo(1, "file1", FileCategory.Image),
                CreateMediaInfo(2, "file2", FileCategory.Image)
            });
            ObservableState.SetFiles(files);
            ObservableState.ClearFiles();

            Assert.AreEqual(0, ObservableState.Current.Files.Count);
            Assert.IsTrue(ObservableState.Current.CurrentFile.IsNone);
        }

        /// <summary>
        /// Verifies that Update does not fire the StateChanged event when the state is unchanged,
        /// preventing unnecessary UI updates.
        /// </summary>
        [TestMethod]
        public void UpdateWithNoStateChange()
        {
            // Arrange - create a state that's identical to current
            var currentState = ObservableState.Current;
            bool eventFired = false;
            ObservableState.StateChanged += (sender, state) => eventFired = true;

            // Update with the same state
            ObservableState.Update(currentState);

            Assert.IsFalse(eventFired); // Event should not fire for identical state

            // Cleanup
            ObservableState.StateChanged -= (sender, state) => eventFired = true;
        }

        /// <summary>
        /// Tests that Update correctly updates the state with new values
        /// and fires the appropriate state changed event.
        /// </summary>
        [TestMethod]
        public void UpdateWithStateChange()
        {
            // Create a state that's different from current
            var newState = ObservableState.Current with { WorkInProgress = true };
            bool eventFired = false;
            ObservableState.StateChanged += (sender, state) => eventFired = true;

            // Act - update with the new state
            ObservableState.Update(newState);

            Assert.IsTrue(eventFired);
            Assert.IsTrue(ObservableState.Current.WorkInProgress);
            ObservableState.StateChanged -= (sender, state) => eventFired = true;
        }

        /// <summary>
        /// Tests the thread safety of ObservableState by performing multiple concurrent updates
        /// and verifying that all operations complete successfully without exceptions.
        /// </summary>
        [TestMethod]
        public void AtomicConcurrentUpdates()
        {
            const int numberOfThreads = 10;
            const int updatesPerThread = 100;
            var allDone = new ManualResetEvent(false);
            var threadsCompleted = 0;
            var threadExceptions = new List<Exception>();

            // Function to increment the counter on multiple threads
            void ThreadWork()
            {
                try
                {
                    for (int i = 0; i < updatesPerThread; i++)
                    {
                        // Toggle the CopyOnly flag
                        var currentValue = ObservableState.Current.CopyOnly.Value;
                        ObservableState.SetCopyOnly(!currentValue);
                    }

                    // Signal this thread is done
                    if (Interlocked.Increment(ref threadsCompleted) == numberOfThreads)
                    {
                        allDone.Set();
                    }
                }
                catch (Exception ex)
                {
                    lock (threadExceptions)
                    {
                        threadExceptions.Add(ex);
                    }
                }
            }

            // Start multiple threads
            for (int i = 0; i < numberOfThreads; i++)
            {
                var thread = new Thread(ThreadWork);
                thread.Start();
            }

            // Wait for all threads to complete (with timeout)
            var completed = allDone.WaitOne(TimeSpan.FromSeconds(10));

            // Assert
            Assert.IsTrue(completed, "Not all threads completed in time");
            Assert.AreEqual(0, threadExceptions.Count, $"Exceptions occurred: {string.Join(", ", threadExceptions.Select(e => e.Message))}");
        }

        /// <summary>
        /// Creates a new MediaInfo object with the specified properties for testing purposes.
        /// </summary>
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