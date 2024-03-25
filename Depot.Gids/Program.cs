using Depot.Common;
using Depot.Common.Navigation;
using Depot.Common.Validation;
using Depot.Common.Workflow;
using Depot.DAL;
using Depot.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;

namespace Depot.Gids;

class Program
{
    private static PagingMenu? consoleMenu;
    private static DepotContext depotContext = new DepotContext();

    static void Main(string[] args)
    {
        Console.WriteLine(Localization.Load_context);
        depotContext.LoadContext();

        while (true)
        {
            var userId = UserInput.GetNumber(Localization.Scan_uw_pas, 1);
            var validation = new UserValidationFlow(depotContext, userId, Role.Gids);
            var message = validation.Validate(out bool valid);

            Console.WriteLine(message);
            if (valid)
            {
                consoleMenu = new PagingMenu($"{(Role)validation.User!.Role} {validation.User!.Name}", Localization.Maak_uw_keuze);
                consoleMenu.SetListItems(CreateTourList());

                consoleMenu.Show();

            }

            Console.WriteLine(Localization.Ongeldige_invoer);
            ResetMenuState();
        }
    }

    private static List<Menu> CreateTourList()
    {
        var resultTours = new List<Menu>();
        var index = 3;

        var today = DateTime.Now;
        var todaysTours = depotContext.Tours.Where(t =>
            t.Start.DayOfYear == today.DayOfYear &&
            t.Start.Year == today.Year &&
            t.Start.TimeOfDay > today.TimeOfDay)
            .OrderBy(q => q.Start).ToList();

        todaysTours.ForEach(t =>
        {
            var tourDetailsMenuItem = new Menu(index.ToString()[0], $"{Localization.Rondleidingen_van} {t.Start.ToString("HH:mm")}", $"{Localization.Aanmeldingen}: {t.RegisteredTickets.Count}/{Globals.Maximum_places}, {Localization.Gestart}: {(t.Departed ? Localization.Ja : Localization.Nee)}", null, consoleMenu);
            tourDetailsMenuItem.AddMenuItem(new Menu('1', Localization.Bekijk_details, Localization.Bekijk_details_van_deze_rondleiding, () => { GetTour(t.Id); }, tourDetailsMenuItem));
            tourDetailsMenuItem.AddMenuItem(new Menu('2', Localization.Toevoegen_bezoeker, Localization.Een_bezoeker_toevoegen_aan_deze_rondleiding, () => { AddVisitor(t.Id); }, tourDetailsMenuItem));
            tourDetailsMenuItem.AddMenuItem(new Menu('3', Localization.Verwijderen_bezoeker, Localization.Een_bezoeker_verwijderen_van_deze_rondleiding, () => { RemoveVisitor(t.Id); }, tourDetailsMenuItem));
            tourDetailsMenuItem.AddMenuItem(new Menu('4', Localization.Start, Localization.Start_Rondleiding, () => { StartTour(t.Id); }, tourDetailsMenuItem));
            tourDetailsMenuItem.AddReturnOrShutdown();

            resultTours.Add(tourDetailsMenuItem);
            index++;
            if (index == 10)
            {
                index = 3;
            }
        });
        return resultTours;
    }

    private static void StartTour(long tourId)
    {
        var startTour = new StartTourFlow(depotContext, tourId);
        if (startTour.Tour!.Departed)
        {
            Console.WriteLine(Localization.Rondleiding_al_gestart);
            ResetMenuState();
            return;
        }

        Console.WriteLine(Localization.Start_Tour_Checkin);

        while (startTour.Tour!.RegisteredTickets.Count > startTour.ConfirmedTickets.Count)
        {
            var ticketNumber = UserInput.GetNumber($"{Localization.Ticket} {startTour.ConfirmedTickets.Count + 1}/{startTour.Tour!.RegisteredTickets.Count}: ", 1);
            var valid = startTour.AddTicket(ticketNumber, out string? message);

            if (message != null)
            {
                Console.WriteLine(message);
            }

            if (!valid)
            {
                ResetMenuState();
                break;
            }
            else
            {
                UpdateMenuState();
                continue;
            }
        }

        startTour.Cleanup();

        // If tickets are available to be added, we try to fill up the tour
        Console.WriteLine(Localization.Plekken_vrij_toevoegen);

        while (Globals.Maximum_places > startTour.ConfirmedTickets.Count)
        {
            var ticketNumber = UserInput.GetNumber($"{Localization.Ticket} {startTour.Tour!.RegisteredTickets.Count + 1}/{Globals.Maximum_places}: ", 1);
            var valid = startTour.AddTicket(ticketNumber, out string? message);

            if (message != null)
            {
                Console.WriteLine(message);
            }

            if (!valid)
            {
                ResetMenuState();
                break;
            }
            else
            {
                UpdateMenuState();
                continue;
            }
        }

        Console.WriteLine(startTour.Validate(out _));
        UpdateMenuState();
        ResetMenuState();
    }

    private static void RemoveVisitor(long tourId)
    {
        var ticketNummer = UserInput.GetNumber(Localization.Scan_uw_ticket, 1);
        var cancel = new CancelReservationFlow(depotContext, ticketNummer, tourId);
        var message = cancel.Validate(out bool valid);

        Console.WriteLine(message);
        if (!valid)
        {
            ResetMenuState();
            return;
        }

        cancel.RemoveTickets();
        Console.WriteLine(Localization.Ticket_verwijderd);
        ResetMenuState();
    }
    private static void AddVisitor(long tourId)
    {
        var ticketNummer = UserInput.GetNumber(Localization.Scan_uw_ticket, 1);
        var create = new CreateReservationFlow(depotContext, ticketNummer);

        if (!create.SetTicketAmount(1) || !create.SetTour(tourId))
        {
            ResetMenuState();
            return;
        }

        Console.WriteLine(create.Validate(out _));
        ResetMenuState();
    }

    private static void GetTour(long tourNr)
    {
        var tour = depotContext.Tours.Where(t => t.Id == tourNr).FirstOrDefault();

        Console.WriteLine($"{Localization.Rondleiding_om} {tour?.Start.ToString("HH:mm")}");
        Console.WriteLine(Localization.Alle_ticketnummers);
        foreach (var ticketNummer in tour!.RegisteredTickets)
        {
            Console.WriteLine($"{Localization.Ticket} {ticketNummer}");
        }

        ResetMenuState();
    }

    public static void ResetMenuState()
    {
        if (consoleMenu != null)
        {
            consoleMenu.SetListItems(CreateTourList());
            Console.WriteLine(Localization.Ga_terug);
            consoleMenu.Reset();
            Console.ReadLine();
        }
    }

    public static void UpdateMenuState()
    {
        consoleMenu?.SetListItems(CreateTourList());
    }
}