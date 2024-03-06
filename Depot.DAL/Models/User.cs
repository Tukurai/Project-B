using Depot.Common;
using System.ComponentModel.DataAnnotations;

namespace Depot.DAL.Models
{
    public class User: DbEntity
    {
        public Role Role { get; set; }
        public string Name{ get; set; } = "";
    }
}
