using MediaOrganiser.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MediaOrganiser.Tests.Domain;

[TestClass]
public class TypesTests
{
    [TestMethod]
    public void FileIdCompareTo()
    {
        // Arrange
        var id1 = new FileId(1);
        var id2 = new FileId(2);
        var id3 = new FileId(3);
        var id1Again = new FileId(1);

        // Act & Assert
        Assert.IsTrue(id1.CompareTo(id2) < 0);  // 1 < 2
        Assert.IsTrue(id2.CompareTo(id1) > 0);  // 2 > 1
        Assert.IsTrue(id3.CompareTo(id2) > 0);  // 3 > 2
        Assert.AreEqual(0, id1.CompareTo(id1Again)); // 1 == 1
        Assert.IsTrue(id1.CompareTo(null) > 0); // Any non-null > null
    }


    [TestMethod]
    public void FileIdEquality()
    {
        var id1 = new FileId(1);
        var id1Again = new FileId(1);
        var id2 = new FileId(2);

        Assert.AreEqual(id1, id1Again);
        Assert.AreNotEqual(id1, id2);
        Assert.IsTrue(id1 == id1Again);
        Assert.IsFalse(id1 == id2);
        Assert.IsFalse(id1 != id1Again);
        Assert.IsTrue(id1 != id2);
    }
}