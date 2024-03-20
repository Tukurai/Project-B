namespace Depot.DAL.Models
{
    public class User : DbEntity
    {
        public int Role { get; set; }
        public string Name { get; set; } = "";
    }
}
