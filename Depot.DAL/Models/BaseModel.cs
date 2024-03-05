namespace Depot.DAL.Models;

public class BaseModel: ISerializeable
{
    public int Id { get; }
    
    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        BaseModel other = (BaseModel)obj;
        return Id == other.Id;
    }
    
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}