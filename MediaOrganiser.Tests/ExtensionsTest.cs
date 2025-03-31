using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediaOrganiser.Core;
using static MediaOrganiser.Core.Types;
using LanguageExt;
using static LanguageExt.Prelude;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using LanguageExt.Common;
using System;
using static MediaOrganiser.Core.AppErrors.ErrorMessages;

namespace MediaOrganiser.Tests.Core
{
    [TestClass]
    public class ExtensionsTests
    {
        /// <summary>
        /// Tests the Separate extension method with a list of numbers
        /// </summary>
        [TestMethod]
        public void Separate()
        {
            var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var (evens, odds) = numbers.Separate(n => n % 2 == 0);

            CollectionAssert.AreEqual(new[] { 2, 4, 6, 8, 10 }, evens.ToArray());
            CollectionAssert.AreEqual(new[] { 1, 3, 5, 7, 9 }, odds.ToArray());
        }

        /// <summary>
        /// Tests the Separate extension method with an empty list
        /// </summary>
        [TestMethod]
        public void SeparateWithEmpty()
        {
            var emptyList = new List<int>();
            var (matches, nonMatches) = emptyList.Separate(n => n > 0);

            Assert.AreEqual(0, matches.Count);
            Assert.AreEqual(0, nonMatches.Count);
        }

        /// <summary>
        /// Tests the HasValue extension method for strings
        /// </summary>
        [TestMethod]
        public void HasValue()
        {
            Assert.IsTrue("test".HasValue());
            Assert.IsTrue("  test  ".HasValue());
            Assert.IsFalse("".HasValue());
            Assert.IsFalse("   ".HasValue());
            Assert.IsFalse((null as string).HasValue());
        }

        /// <summary>
        /// Tests successful Safe operation
        /// </summary>
        [TestMethod]
        public void SafeSuccess() =>
            Assert.AreEqual(42, IO.lift(() => 42).Safe().Run());

        /// <summary>
        /// Tests that Safe wraps exceptions properly
        /// </summary>
        [TestMethod]
        public void SafeGivesExpectedException() =>
            TestIOException<InvalidOperationException, int>(io => io.Safe(), ThereWasAProblem);

        /// <summary>
        /// Tests that HandleUnauthorised correctly handles UnauthorizedAccessException
        /// </summary>
        [TestMethod]
        public void UnauthorisedException() =>
            TestIOException<UnauthorizedAccessException, int>(
                io => io.HandleUnauthorised(@"C:\test\path"),
                UnauthorisedAccessPrefix);

        /// <summary>
        /// Tests that unexpected exception types throw if not Safe()
        /// </summary>
        [TestMethod]
        public void UnexpectedExceptionThrows() =>
            Assert.ThrowsException<ArgumentException>(
                () => IO.lift<int>(() => { throw new ArgumentException("Test error"); }).Run());                       

        /// <summary>
        /// Tests that HandleFileNotFound correctly handles FileNotFoundException
        /// </summary>
        [TestMethod]
        public void FileNotFoundException() =>
            TestIOException<FileNotFoundException, int>(
                io => io.HandleFileNotFound(@"c:\test\temp.jpg"),
                FileNotFoundPrefix);

        /// <summary>
        /// Tests that HandleDirectoryNotFound correctly handles DirectoryNotFoundException
        /// </summary>
        [TestMethod]
        public void DirectoryNotFoundException() =>
            TestIOException<DirectoryNotFoundException, int>(
                io => io.HandleDirectoryNotFound(@"C:\test\directory"),
                DirectoryNotFoundPrefix);

        /// <summary>
        /// Helper method to test IO exception handling
        /// </summary>
        /// <typeparam name="T">Exception type to test</typeparam>
        /// <typeparam name="S">Return type of the IO operation</typeparam>
        /// <param name="operationHandler">Handler function to test</param>
        /// <param name="expectedMessage">Expected message in the wrapped exception</param>
        void TestIOException<T, S>(Func<IO<S>, IO<S>> operationHandler, string expectedMessage) where T : Exception, new()
        {
            var exception = Assert.ThrowsException<WrappedErrorExpectedException>(
                () => operationHandler(IO.lift<S>(() => { throw new T(); })).Run());

            Assert.IsTrue(exception.Message.Contains(expectedMessage));
        }

        /// <summary>
        /// Tests SeparateUserErrors when only user errors are present
        /// </summary>
        [TestMethod]
        public void SeparateErrors()
        {
            var errors = toSeq(new Error[]{
                    Error.New(AppErrors.DisplayErrorCode, "User error 1"),
                    Error.New("Non error 2")
                });

            var (userErrors, unexpectedErrors) = errors.SeparateUserErrors();

            Assert.AreEqual(1, userErrors.Count);
            Assert.AreEqual(1, unexpectedErrors.Count);
            Assert.AreEqual("User error 1", userErrors.First().Message);
            Assert.AreEqual("Non error 2", unexpectedErrors.First().Message);
        }

        /// <summary>
        /// Tests basic functionality of ExtractUserErrors
        /// </summary>
        [TestMethod]
        public void ExtractErrors()
        {
            // This is a simplified test that doesn't test the full functionality
            // due to the complexity of mocking and testing the ExtractUserErrors method
            var operations = toSeq(new IO<int>[] {
                    IO.pure(1),
                    IO.fail<int>(AppErrors.ThereWasAProblem(Error.New("Test inner")))});

            try
            {
                var (succs, userErrors) = operations.ExtractUserErrors().Run();
                Assert.AreEqual(1, succs.Count);
                Assert.AreEqual(1, userErrors.Count);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected success but got exception: {ex.Message}");
            }
        }
    }
}
