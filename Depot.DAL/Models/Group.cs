
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Depot.DAL.Models
{
    public class Group : DbEntity
    {
        public int GroupOwnerId { get; set; }
        public List<int> TicketIds { get; set; } = new List<int>();
    }

}
