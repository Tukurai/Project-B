using Depot.Common;
using Depot.Common.Navigation;
using Depot.Common.Validation;
using Depot.DAL;
using Depot.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Depot.Kiosk;

class Program
{
    private static Menu? consoleMenu;
    private static DepotContext depotContext = new DepotContext();

    static void Main(string[] args)
    {
        Console.WriteLine(Localization.Load_context);
        depotContext.LoadJson();

        consoleMenu = new Menu(Localization.Kiosk, Localization.Maak_uw_keuze);

        var reserveren = new Menu('1', Localization.Reserveren, Localization.Uw_rondleiding_reserveren, () => { StartReservation(); });
        consoleMenu.AddMenuItem(reserveren);

        var wijzigen = new Menu('2', Localization.Wijzigen, Localization.Uw_rondleiding_wijzigen, EditReservation);
        consoleMenu.AddMenuItem(wijzigen);

        var annuleren = new Menu('3', Localization.Annuleren, Localization.Uw_rondleiding_annuleren, CancelReservation);
        consoleMenu.AddMenuItem(annuleren);

        var bekijken = new Menu('4', Localization.Bekijken, Localization.Uw_rondleiding_bekijken, ViewReservation);
        consoleMenu.AddMenuItem(bekijken);

        consoleMenu.Show();
    }

    private static void ViewReservation()
    {
        var ticketNumber = UserInput.GetNumber(Localization.Scan_uw_ticket, min: 1);
        var reservation = depotContext.Tours.FirstOrDefault(t => t.RegisteredTickets.Contains(ticketNumber));

        if (reservation != null)
        {
            Console.WriteLine($"{Localization.Uw_rondleiding_is_om} {reservation.Start.ToString("HH:mm")}.");
        }
        else
        {
            Console.WriteLine(Localization.Aanmelding_niet_gevonden);
        }

        Console.WriteLine(Localization.Ga_terug);
        consoleMenu?.Reset();
        Console.ReadLine();
    }

    private static void EditReservation()
    {
        int ticketNumber = UserInput.GetNumber(Localization.Scan_uw_ticket, min: 1);
        var reservation = depotContext.Tours.FirstOrDefault(t => t.RegisteredTickets.Contains(ticketNumber));

        if (reservation == null)
        {
            Console.WriteLine(Localization.Aanmelding_niet_gevonden);
            Console.WriteLine(Localization.Ga_terug);
            consoleMenu?.Reset();
            Console.ReadLine();
            return;
        }
        else
        {
            Console.WriteLine($"{Localization.Uw_rondleiding_is_om} {reservation.Start.ToString("HH:mm")}.");
            Console.WriteLine(Localization.Reservering_Wijzigen);
            var response = Console.ReadLine();

            if (response == "y")
            {
                reservation.RegisteredTickets.Remove(ticketNumber);
                StartReservation(ticketNumber);
            }
            else
            {
                Console.WriteLine(Localization.Reservering_niet_gewijzigd);
                Console.WriteLine(Localization.Ga_terug);
                consoleMenu?.Reset();
                Console.ReadLine();
            }
        }
    }

    private static void CancelReservation()
    {
        var ticketNumber = UserInput.GetNumber(Localization.Scan_uw_ticket, min: 1);
        var reservation = depotContext.Tours.FirstOrDefault(t => t.RegisteredTickets.Contains(ticketNumber));

        if (reservation != null)
        {
            Console.WriteLine($"{Localization.Uw_rondleiding_is_om} {reservation.Start.ToString("HH:mm")}.");
            Console.WriteLine(Localization.Annulering_bevestigen);

            var userInput = Console.ReadLine() ?? "";
            if (userInput == "y")
            {
                reservation.RegisteredTickets.Remove(ticketNumber);
                depotContext.SaveChanges();
                Console.WriteLine(Localization.Reservering_is_geannuleerd);
            }
            else
            {
                Console.WriteLine(Localization.Reservering_niet_geannuleerd);
            }
        }
        else
        {
            Console.WriteLine(Localization.Aanmelding_niet_gevonden);
        }

        Console.WriteLine(Localization.Ga_terug);
        consoleMenu?.Reset();
        Console.ReadLine();
    }

    private static void StartReservation(int? ticketnummer = null)
    {
        int amountOfTickets = 1;
        if (ticketnummer == null)
        {
            amountOfTickets = UserInput.GetNumber(Localization.Hoeveel_plaatsen_wilt_u_reserveren, 0, Globals.Maximum_Plekken);
        }

        Tour? tour = GetTour(amountOfTickets);
        if (tour == null)
        {
            Console.WriteLine(Localization.Ga_terug);
            consoleMenu?.Reset();
            Console.ReadLine();
            return;
        }

        List<int> ticketNumbers = new List<int>();
        if (ticketnummer != null)
        {
            ticketNumbers.Add(ticketnummer.Value);
        }
        else
        {
            for (int i = 0; i < amountOfTickets; i++)
            {
                ticketNumbers.Add(UserInput.GetNumber(Localization.Scan_uw_ticket, min: 1));
            }
        }
        depotContext.SaveChanges();

        Console.WriteLine(Localization.Ga_terug);
        consoleMenu?.Reset();
        Console.ReadLine();
    }

    private static Tour? GetTour(int amountOfTickets)
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

        int tourIndex = UserInput.GetNumber(Localization.Welke_rondleiding_wilt_u_reserveren, 0, todaysTours.Count - 1);
        var tour = todaysTours[tourIndex];

        return todaysTours[tourIndex];
    }
}