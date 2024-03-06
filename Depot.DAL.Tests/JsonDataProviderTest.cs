using Microsoft.VisualStudio.TestTools.UnitTesting;
using Depot.DAL;
using Depot.DAL.Models;
using System.IO;
using Newtonsoft.Json;

public class MockSerializeable : DbEntity
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
}