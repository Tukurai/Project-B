using Microsoft.VisualStudio.TestTools.UnitTesting;
using Depot.DAL;
using Depot.DAL.Models;
using System.IO;
using Newtonsoft.Json;

public class MockSerializeable : ISerializeable
{
    public int Id { get; set; }
}

[TestClass]
public class JsonDataProviderTests
{
    private JsonDataProvider<MockSerializeable> _provider;
    private string _filePath;

    [TestInitialize]
    public void TestInitialize()
    {
        // Arrange
        _filePath = "JsonFiles/test.json";
        _provider = new JsonDataProvider<MockSerializeable>("test");

        // Clean up any existing test file
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }

    [TestMethod]
    public void LoadAllFromProvider_Can_Load_Data()
    {
        // Arrange
        var expectedData = new List<MockSerializeable> { new MockSerializeable { Id = 1 } };
        File.WriteAllText(_filePath, JsonConvert.SerializeObject(expectedData));

        // Act
        _provider.LoadAllFromProvider();

        // Assert
        Assert.AreEqual(expectedData[0].Id, _provider.Data[0].Id);
    }

    [TestMethod]
    public void SaveToProvider_DataSavedToFile()
    {
        // Arrange
        var expectedData = new List<MockSerializeable> { new MockSerializeable { Id = 1 } };
        _provider.Data.AddRange(expectedData);

        // Act
        _provider.SaveToProvider(expectedData[0]);

        // Assert
        var actualData = JsonConvert.DeserializeObject<List<MockSerializeable>>(File.ReadAllText(_filePath));
        Assert.IsTrue(expectedData[0].Id == actualData![0].Id);
    }
}