namespace Depot.DAL.Models
{
    public class Tour : DbEntity
    {
        public DateTime Start { get; set; }
        public List<long> RegisteredTickets { get; set; } = new List<long>();
        public bool Departed { get; set; } = false;
    }

}
