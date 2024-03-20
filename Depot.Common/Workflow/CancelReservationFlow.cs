using Depot.DAL;
using Depot.DAL.Models;

namespace Depot.Common.Workflow
{
    public class CancelReservationFlow : Workflow
    {
        public Tour? Tour { get; set; }
        public Group? Group { get; set; }
        public long? TicketNumber { get; set; }

        public CancelReservationFlow(DepotContext context, long? ticketNumber) : base(context)
        {
            Tour = context.Tours.FirstOrDefault(t => t.RegisteredTickets.Contains(ticketNumber!.Value));
            TicketNumber = ticketNumber;
        }

        public override string Validate(out bool valid)
        {
            valid = false;

            if (Tour == null)
            {
                return Localization.Aanmelding_niet_gevonden;
            }

            if (Tour.Departed)
            {
                return Localization.Uw_kunt_uw_reservering_niet_meer_aanpassen;
            }

            Group = Context.Groups.FirstOrDefault(g => g.TicketIds.Contains(TicketNumber!.Value));
            if (Group != null && Group.GroupOwnerId != TicketNumber)
            {
                return Localization.Uw_kunt_uw_reservering_niet_annuleren;
            }

            valid = true;
            return $"{Localization.Uw_rondleiding_is_om} {Tour.Start.ToString("HH:mm")}.";
        }

        public void RemoveTickets()
        {
            Group!.TicketIds.ForEach(t => Tour!.RegisteredTickets.Remove(t));
            Context.Groups.Remove(Group);
            Context.SaveChanges();
        }
    }
}
