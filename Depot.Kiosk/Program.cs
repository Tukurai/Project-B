﻿using Depot.Common;
using Depot.Common.Navigation;
using Depot.Common.Validation;
using Depot.Common.Workflow;
using Depot.DAL;
using Depot.DAL.Models;
using System;
using System.Collections.Generic;
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
        Console.WriteLine(view.GetOutput());
        ResetMenuState();
    }

    private static void EditReservation()
    {
        var reservation = depotContext.Tours.FirstOrDefault(t => t.RegisteredTickets.Contains(TicketNumber!.Value));

        if (reservation == null)
        {
            Console.WriteLine(Localization.Aanmelding_niet_gevonden);

            ResetMenuState();
            return;
        }

        if (reservation.Departed)
        {
            Console.WriteLine(Localization.Uw_kunt_uw_reservering_niet_meer_aanpassen);
            ResetMenuState();
            return;
        }

        var group = depotContext.Groups.FirstOrDefault(g => g.TicketIds.Contains(TicketNumber!.Value));
        if (group != null && group.GroupOwnerId != TicketNumber)
        {
            Console.WriteLine(Localization.Uw_kunt_uw_reservering_niet_annuleren);
            ResetMenuState();
            return;
        }

        Console.WriteLine($"{Localization.Uw_rondleiding_is_om} {reservation.Start.ToString("HH:mm")}.");
        Console.WriteLine(Localization.Reservering_Wijzigen);
        var response = Console.ReadLine();

        if (response == "y")
        {
            group.TicketIds.ForEach(t => reservation.RegisteredTickets.Remove(t));
            depotContext.Groups.Remove(group);

            StartReservation();
        }
        else
        {
            Console.WriteLine(Localization.Reservering_niet_gewijzigd);

        }

        ResetMenuState();
    }

    private static void CancelReservation()
    {
        var reservation = depotContext.Tours.FirstOrDefault(t => t.RegisteredTickets.Contains(TicketNumber!.Value));

        if (reservation == null)
        {
            Console.WriteLine(Localization.Aanmelding_niet_gevonden);
            ResetMenuState();
            return;
        }

        if (reservation.Departed)
        {
            Console.WriteLine(Localization.Uw_kunt_uw_reservering_niet_meer_aanpassen);
            ResetMenuState();
            return;
        }

        var group = depotContext.Groups.FirstOrDefault(g => g.TicketIds.Contains(TicketNumber!.Value));
        if (group != null && group.GroupOwnerId != TicketNumber)
        {
            Console.WriteLine(Localization.Uw_kunt_uw_reservering_niet_annuleren);
            ResetMenuState();
            return;
        }

        Console.WriteLine($"{Localization.Uw_rondleiding_is_om} {reservation.Start.ToString("HH:mm")}.");
        Console.WriteLine(Localization.Annulering_bevestigen);

        var userInput = Console.ReadLine() ?? "";
        if (userInput == "y")
        {
            group!.TicketIds.ForEach(t => reservation.RegisteredTickets.Remove(t));
            depotContext.Groups.Remove(group);

            depotContext.SaveChanges();
            Console.WriteLine(Localization.Reservering_is_geannuleerd);
        }
        else
        {
            Console.WriteLine(Localization.Reservering_niet_geannuleerd);
        }

        ResetMenuState();
    }

    private static void StartReservation(Group? group = null)
    {
        long? amountOfTickets = group?.TicketIds.Count ?? UserInput.GetNumber(Localization.Hoeveel_plaatsen_wilt_u_reserveren, 1, Globals.Maximum_Plekken);
        if (amountOfTickets == null)
        {
            ResetMenuState();
            return;
        }

        Tour? tour = GetTour(amountOfTickets.Value);
        if (tour == null)
        {
            ResetMenuState();
            return;
        }

        List<long> ticketNumbers = new List<long>() { TicketNumber!.Value };
        if (group != null)
        {
            ticketNumbers = group.TicketIds;
        }

        while (ticketNumbers.Count < amountOfTickets)
        {
            var additionalTicket = UserInput.GetNumber(Localization.Scan_uw_ticket, min: 1);
            if (additionalTicket == null)
            {
                amountOfTickets = ticketNumbers.Count;
                break;
            }

            bool hasReservation = depotContext.Tours.Any(tour => tour.RegisteredTickets.Contains(additionalTicket.Value));

            if (hasReservation)
            {
                Console.WriteLine(Localization.Ticket_heeft_al_een_reservering);
                continue;
            }

            if (ticketNumbers.Contains(additionalTicket.Value))
            {
                Console.WriteLine(Localization.Ticket_zit_al_in_uw_groep);
                continue;
            }

            ticketNumbers.Add(additionalTicket.Value);
        }

        var newGroup = new Group() { GroupOwnerId = TicketNumber!.Value, TicketIds = ticketNumbers };
        depotContext.Groups.Add(newGroup);

        tour.RegisteredTickets.AddRange(ticketNumbers);
        depotContext.SaveChanges();

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

    private static Tour? GetTour(long amountOfTickets)
    {
        var today = DateTime.Now;
        var todaysTours = depotContext.Tours.Where(t =>
            t.Start.DayOfYear == today.DayOfYear &&
            t.Start.Year == today.Year &&
            t.Start.TimeOfDay > today.TimeOfDay &&
            (Globals.Maximum_Plekken - t.RegisteredTickets.Count) >= amountOfTickets)
            .OrderBy(q => q.Start).ToList();

        if (todaysTours.Count <= 0)
        {
            Console.WriteLine(Localization.Geen_rondleidingen_meer);
            return null;
        }

        Console.WriteLine(Localization.Rondleidingen_van_vandaag);
        for (int i = 0; i < todaysTours.Count; i++)
        {
            Console.WriteLine($"{i}. {Localization.Rondleiding_om} {todaysTours[i].Start.ToString("HH:mm")}");
            Console.ResetColor();
        }

        var tourIndex = UserInput.GetNumber(Localization.Welke_rondleiding_wilt_u_reserveren, 0, todaysTours.Count - 1);
        if (tourIndex == null)
        {
            return null;
        }

        var tour = todaysTours[(int)tourIndex!.Value];

        return todaysTours[(int)tourIndex!.Value];
    }
}