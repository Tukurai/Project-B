using Depot.Common;

namespace Depot.DAL.Models
{
    public class User: BaseModel
    {
        public Role Role { get; set; }
        public string Name{ get; set; } = "";
    }
}
