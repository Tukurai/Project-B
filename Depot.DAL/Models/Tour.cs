using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Depot.DAL.Models
{
    public class Tour: BaseModel
    {
        public DateTime Start { get; set; }
        public List<int> Registrations { get; set; } = new List<int>();
        public List<int> Queue { get; set; } = new List<int>();
    }
    
}
