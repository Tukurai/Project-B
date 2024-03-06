
using System.ComponentModel.DataAnnotations;

namespace Depot.DAL.Models
{
    public class Tour : DbEntity
    {
        public DateTime Start { get; set; }
        public List<int> Registrations { get; set; } = new List<int>();
        public List<int> Queue { get; set; } = new List<int>();
    }
    
}
