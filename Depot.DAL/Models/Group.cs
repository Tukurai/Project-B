﻿namespace Depot.DAL.Models
{
    public class Group : DbEntity
    {
        public long GroupOwnerId { get; set; }
        public List<long> TicketIds { get; set; } = new List<long>();
    }

}
