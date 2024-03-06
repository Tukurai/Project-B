using Depot.DAL.Models;

namespace Depot.DAL;

public abstract class BaseDataProvider<T> : IDataProvider<T> where T : DbEntity
{
    
    protected List<T> Data { get; set;} = new List<T>();
    // Default implementations of Crud functions that operate in memory
    public override void Create(T entity)
    {
        // Check if already exists
        if (Data.Any(e => e.Id == entity.Id))
        {
            // TODO Log that the entity already exists?
            return;
        }

        Data.Add(entity);
        SaveToProvider(entity);
    }

    public override T? Read(int id)
    {
        var entity = Data.FirstOrDefault(e => e.Id == id);
        return entity ?? default;
    }

    public override void Update(T entity)
    {
        var index = Data.FindIndex(e => e.Id == entity.Id);
        if (index == -1) return;
        Data[index] = entity;
        UpdateToProvider(entity);
    }

    public override void Delete(T entity)
    {
        if (Data.Contains(entity))
        {
            Data.Remove(entity);
            DeleteToProvider(entity);
        }
        else
        {
            // TODO Log that the entity does not exist?
        }
    }

    // Custom functions that write to the persistence provider
    protected abstract void SaveToProvider(T entity);
    protected abstract void UpdateToProvider(T entity);
    protected abstract void DeleteToProvider(T entity);
    protected abstract void LoadAllFromProvider();
}