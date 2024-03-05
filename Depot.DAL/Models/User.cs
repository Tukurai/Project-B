using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Depot.DAL.Enums;

namespace Depot.DAL.Models
{
    public class User: BaseModel
    {
        public Role Role { get; set; }
        public string Name{ get; set; } = "";
    }
}
