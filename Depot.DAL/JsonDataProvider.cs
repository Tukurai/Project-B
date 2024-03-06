using Depot.DAL.Models;
using Newtonsoft.Json;

namespace Depot.DAL;

public sealed class JsonDataProvider<T> : BaseDataProvider<T> where T : DbEntity
{
    public string FilePath { get;}

    public JsonDataProvider(string fileName)
    {
        FilePath = $"JsonFiles/{fileName.ToLower()}.json";
        LoadAllFromProvider();
    }

    protected override void LoadAllFromProvider()
    {
        if (File.Exists(FilePath))
        {
            string json = File.ReadAllText(FilePath);
            // TODO Maybe check if file is empty and warn user / populate file with mock data?
            Data = JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
        }
        else
        {
            // TODO Log file not found (and populate with mock data)
            Data = new List<T>();
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(Data));
        }
    }

    protected override void SaveToProvider(T entity)
    {
        SaveChanges();
    }

    protected override void UpdateToProvider(T entity)
    {
        SaveChanges();
    }

    protected override void DeleteToProvider(T entity)
    {
        SaveChanges();
    }

    private void SaveChanges()
    {
        File.WriteAllText(FilePath, JsonConvert.SerializeObject(Data));
    }
}