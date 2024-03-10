using Depot.DAL.Models;
using System.Collections.Generic;

namespace Depot.DAL;

public abstract class IDataProvider<T> where T : DbEntity
{
    protected List<T> Data { get; set;}

    public abstract void Create(T entity);
    public abstract T? Read(int id);
    public abstract void Update(T entity);
    public abstract void Delete(T entity);
    
}