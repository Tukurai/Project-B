using Depot.DAL;
using Depot.DAL.Models;

namespace Depot.Common.Workflow
{
    public class CreateReservationFlow : Workflow
    {
        public Tour? Tour { get; set; }
        public Group? Group { get; set; }
        public List<long> TicketNumbers { get; set; } = new List<long>();
        public int TicketAmount { get; set; }

        public CreateReservationFlow(DepotContext context, long? ticketNumber) : base(context)
        {
            TicketNumbers.Add(ticketNumber!.Value);
        }

        public bool SetTicketAmount(int? ticketAmount)
        {
            if ((ticketAmount ?? 0) < 1)
            {
                return false;
            }

            TicketAmount = ticketAmount!.Value;
            return true;
        }

        public string AddTicket(long ticketNumber)
        {
            if (Context.Tours.Any(tour => tour.RegisteredTickets.Contains(ticketNumber)))
            {
                return Localization.Ticket_heeft_al_een_reservering;
            }

            if (TicketNumbers.Contains(ticketNumber))
            {
                return Localization.Ticket_zit_al_in_uw_groep;
            }

            TicketNumbers.Add(ticketNumber);
            return Localization.Ticket_is_toegevoegd;
        }

        public bool CopyTicketsFromGroup(Group? group)
        {
            if (group == null)
            {
                return false;
            }

            TicketNumbers = group!.TicketIds;
            return true;
        }

        public bool SetTour(Tour? tour)
        {
            if (tour == null)
            {
                return false;
            }

            Tour = tour;
            return true;
        }

        public override string Validate(out bool valid)
        {
            var newGroup = new Group() { GroupOwnerId = TicketNumbers.First(), TicketIds = TicketNumbers };
            Context.Groups.Add(newGroup);

            Tour!.RegisteredTickets.AddRange(TicketNumbers);
            Context.SaveChanges();

            valid = true;
            return Localization.Uw_rondleiding_is_gereserveerd;
        }
    }
}
