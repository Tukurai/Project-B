using Depot.Common;
using Depot.Common.Navigation;
using Depot.Common.Validation;
using Depot.Common.Workflow;
using Depot.DAL;
using Depot.DAL.Models;
using System;
using System.Linq;

namespace Depot.Kiosk;

class Program
{
    private static DepotContext depotContext = new DepotContext();
    private static Menu? consoleMenu;

    public static bool Shutdown { get; set; } = false;

    public static long? TicketNumber { get; set; }

    static void Main(string[] args)
    {
        Console.WriteLine(Localization.Load_context);
        depotContext.LoadContext();

        Menu reserveringsMenu = new Menu(Localization.Kiosk, Localization.Maak_uw_keuze);
        reserveringsMenu.ReplaceShutdown(() =>
        {
            reserveringsMenu.IsShowing = false;
            Console.Clear();
        });

        var reserveren = new Menu('1', Localization.Reserveren, Localization.Uw_rondleiding_reserveren, () => { StartReservation(); });
        reserveringsMenu.AddMenuItem(reserveren);

        Menu beheerReserveringMenu = new Menu(Localization.Kiosk, Localization.Maak_uw_keuze);
        beheerReserveringMenu.ReplaceShutdown(() =>
        {
            beheerReserveringMenu.IsShowing = false;
            Console.Clear();
        });

        var wijzigen = new Menu('1', Localization.Wijzigen, Localization.Uw_rondleiding_wijzigen, EditReservation);
        beheerReserveringMenu.AddMenuItem(wijzigen);
        var annuleren = new Menu('2', Localization.Annuleren, Localization.Uw_rondleiding_annuleren, CancelReservation);
        beheerReserveringMenu.AddMenuItem(annuleren);
        var bekijken = new Menu('3', Localization.Bekijken, Localization.Uw_rondleiding_bekijken, ViewReservation);
        beheerReserveringMenu.AddMenuItem(bekijken);

        while (!Shutdown)
        {
            TicketNumber = UserInput.GetNumber(Localization.Scan_uw_ticket, 1);
            if (TicketNumber == null)
            {
                continue;
            }


            var reservation = depotContext.Tours.FirstOrDefault(t => t.RegisteredTickets.Contains(TicketNumber!.Value));

            if (reservation != null)
            {
                consoleMenu = beheerReserveringMenu;
                beheerReserveringMenu.Show();
            }
            else
            {
                consoleMenu = reserveringsMenu;
                reserveringsMenu.Show();
            }
        }
    }

    private static void ViewReservation()
    {
        var view = new ViewReservationFlow(depotContext, TicketNumber);
        Console.WriteLine(view.Validate(out _));
        ResetMenuState();
    }

    private static void EditReservation()
    {
        CancelOrEditReservation(true);
    }

    private static void CancelReservation()
    {
        CancelOrEditReservation(false);
    }

    private static void CancelOrEditReservation(bool edit = false)
    {
        var cancel = new CancelReservationFlow(depotContext, TicketNumber);
        var message = cancel.Validate(out bool valid);

        Console.WriteLine(message);
        if (!valid)
        {
            ResetMenuState();
            return;
        }

        Console.WriteLine(edit ? Localization.Reservering_Wijzigen : Localization.Annulering_bevestigen);
        var response = Console.ReadLine();

        if (response == "y")
        {
            cancel.RemoveTickets();
            if (edit)
            {
                StartReservation(cancel.Group);
            }
            else
            {
                Console.WriteLine(Localization.Reservering_is_geannuleerd);
            }
            ResetMenuState();
            return;
        }

        Console.WriteLine(edit ? Localization.Reservering_niet_gewijzigd : Localization.Reservering_niet_geannuleerd);
        ResetMenuState();
    }

    private static void StartReservation(Group? group = null)
    {
        var create = new CreateReservationFlow(depotContext, TicketNumber);

        int? amountOfTickets = group?.TicketIds.Count ?? (int?)UserInput.GetNumber(Localization.Hoeveel_plaatsen_wilt_u_reserveren, 1, Globals.Maximum_places);

        if (!create.SetTicketAmount(amountOfTickets) || !create.SetTour(UserInput.GetTour(create.TicketAmount, depotContext)))
        {
            ResetMenuState();
            return;
        }

        create.CopyTicketsFromGroup(group);

        while (create.TicketNumbers.Count < create.TicketAmount)
        {
            var ticketNumber = UserInput.GetNumber(Localization.Scan_uw_ticket, min: 1);
            if (ticketNumber == null)
            {
                create.TicketAmount = create.TicketNumbers.Count;
                break;
            }

            Console.WriteLine(create.AddTicket(ticketNumber!.Value));
        }

        Console.WriteLine(create.Validate(out _));
        ResetMenuState();
    }

    public static void ResetMenuState()
    {
        if (consoleMenu != null)
        {
            Console.WriteLine(Localization.Ga_terug);
            consoleMenu.Reset();
            consoleMenu.IsShowing = false;
            Console.ReadLine();
            Console.Clear();
        }
    }
}