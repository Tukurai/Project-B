using Depot.DAL;
using Depot.DAL.Models;

namespace Depot.Common.Workflow
{
    public class ViewReservationFlow : Workflow
    {
        public Tour? Tour { get; set; }

        public ViewReservationFlow(DepotContext context, long? ticketNumber) : base(context)
        {
            Tour = context.Tours.FirstOrDefault(t => t.RegisteredTickets.Contains(ticketNumber!.Value));
        }

        public override string Validate(out bool valid)
        {
            if (Tour != null)
            {
                valid = true;
                return $"{Localization.Uw_rondleiding_is_om} {Tour!.Start.ToString("HH:mm")}.";
            }
            else
            {
                valid = false;
                return Localization.Aanmelding_niet_gevonden;
            }
        }
    }
}
