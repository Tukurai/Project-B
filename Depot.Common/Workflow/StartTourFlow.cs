using Depot.Common.Validation;
using Depot.DAL;
using Depot.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Depot.Common.Workflow
{
    public class StartTourFlow : Workflow
    {
        public Tour? Tour { get; set; }
        public List<long> ConfirmedTickets { get; set; }

        public StartTourFlow(DepotContext context, long? tourId) : base(context)
        {
            Tour = context.Tours.FirstOrDefault(t => t.Id == tourId);
        }

        public override string Validate(out bool valid)
        {
            Tour!.Departed = true;
            Context.SaveChanges();

            valid = true;
            return Localization.Rondleiding_Gestart;
        }

        public bool AddTicket(long? ticketNumber, out string? message)
        {
            message = null;

            if (ticketNumber == null)
            {
                return false;
            }

            if (Tour!.RegisteredTickets.Contains(ticketNumber.Value))
            {
                ConfirmedTickets.Add(ticketNumber.Value);
            }
            else if (Tour!.RegisteredTickets.Count < Globals.Maximum_places)
            {
                message = Localization.Ticket_niet_in_reserveringen;
                Tour!.RegisteredTickets.Add(ticketNumber.Value);
                Context.SaveChanges();
            }

            if (Tour!.RegisteredTickets.Count >= Globals.Maximum_places)
            {
                message = Localization.Rondleiding_Vol;
            }

            return true;
        }

        public void Cleanup()
        {
            // Remove absent tickets from the tour
            var presentTickets = Tour!.RegisteredTickets.Intersect(ConfirmedTickets).ToList();
            Tour!.RegisteredTickets = presentTickets;
            Context.SaveChanges();
        }
    }
}
