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

        public string GetOutput()
        {
            if (Tour != null)
            {
                return $"{Localization.Uw_rondleiding_is_om} {Tour!.Start.ToString("HH:mm")}.";
            }
            else
            {
                return Localization.Aanmelding_niet_gevonden;
            }
        }
    }
}
