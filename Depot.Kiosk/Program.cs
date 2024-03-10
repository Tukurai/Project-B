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
        Console.WriteLine("Loading context data");
        depotContext.LoadJson();

        consoleMenu = new Menu("Kiosk", "Maak uw keuze uit het menu hieronder:");

        var reserveren = new Menu('1', "Reservering maken", "Maak een reservering voor een rondleiding.", StartReservation);
        consoleMenu.AddMenuItem(reserveren);

        var annuleren = new Menu('2', "Reservering annuleren", "Annuleer een reservering voor een rondleiding.", CancelReservation);
        consoleMenu.AddMenuItem(annuleren);

        var testSubmenu = new Menu('3', "Extra menu", "Hier vind je nog meer opties.");
        testSubmenu.AddMenuItem(new Menu('1', "Test", "Dit is een test.", () => { Console.WriteLine("AAB 1"); Console.ReadLine(); }));
        testSubmenu.AddMenuItem(new Menu('2', "Test", "Dit is een test.", () => { Console.WriteLine("AAB 2"); Console.ReadLine(); }));
        testSubmenu.AddMenuItem(new Menu('3', "Test", "Dit is een test.", () => { Console.WriteLine("AAB 3"); Console.ReadLine(); }));
        testSubmenu.AddMenuItem(new Menu('4', "Test", "Dit is een test.", () => { Console.WriteLine("AAB 4"); Console.ReadLine(); }));
        consoleMenu.AddMenuItem(testSubmenu);

        consoleMenu.Show();
    }

    private static void Close()
    {
        if (consoleMenu != null)
        {
            consoleMenu.IsShowing = false;
        }
    }

    private static void StartReservation()
    {
        var amountOfTickets = UserInput.GetNumber("Hoeveel plaatsen wilt u reserveren? Voor elke plek wordt een ticketnummer gevraagd. (Maximaal 13)", 0, 13);
        var tourObj = GetTour(amountOfTickets);

        List<int> ticketNumbers = new List<int>();
        for (int i = 0; i < amountOfTickets; i++)
        {
            ticketNumbers.Add(UserInput.GetNumber("Wat is uw Ticketnummer?", min: 1));
        }

        foreach (var ticketNumber in ticketNumbers)
        {
            tourObj.Registrations.Add(ticketNumber);
        }

        Console.WriteLine($"Uw reservering is geplaatst voor tour {tourObj.Id}, met {ticketNumbers.Count()} mensen.");
        Console.WriteLine("Druk op enter om terug naar het hoofdmenu te gaan.");
        consoleMenu?.Reset();
        Console.ReadLine();
    }

    private static void CancelReservation()
    {
        var ticketNumber = UserInput.GetNumber("Wat is uw Ticketnummer?", min: 1);

        CancelTour(ticketNumber);
    }

    private static void CancelTour(int ticketNumber)
    {
        // TODO: Get tour reservation data here...
        // -- bool set to true for testing
        bool foundReservation = true;

        if (foundReservation)
        {
            Console.WriteLine("Reservering voor (tourId) van (tijd) gevonden.");
            Console.WriteLine("Weet u zeker dat u uw reservering wil annuleren? (y/n)");

            var userInput = Console.ReadLine() ?? "";
            if (userInput == "y")
            {
                // TODO: Cancel tour here, remove from json data...
                Console.WriteLine($"Uw reservering voor (tourId) van (tijd) is geannuleerd.");
            }
            else
            {
                Console.WriteLine("Reservering niet geannuleerd.");
            }

            Console.WriteLine("Druk op enter om terug naar het hoofdmenu te gaan.");
            consoleMenu?.Reset();
            Console.ReadLine();
        }
        else
        {
            Console.WriteLine("Geen reservering gevonden voor het opgegeven ticketnummer.");
        }
    }

    private static Tour GetTour(int amountOfTickets)
    {
        var today = DateTime.Now;
        var todaysTours = depotContext.Tours.Where(t => t.Start.DayOfYear == today.DayOfYear && t.Start.Year == today.Year).ToList();

        Console.WriteLine("Rondleidingen van vandaag:");
        for (int i = 0; i < todaysTours.Count; i++)
        {
            int vrijePlekken = 13 - todaysTours[i].Registrations.Count;
            bool ruimte = vrijePlekken >= amountOfTickets;

            Console.WriteLine($"{i} Rondleiding {todaysTours[i].Id}: {todaysTours[i].Start}. ({vrijePlekken} plekken vrij) (plek vrij {ruimte})");
        }

        int tourIndex = UserInput.GetNumber("Welke rondleiding wilt u reserveren?", 0, todaysTours.Count - 1);
        return todaysTours[tourIndex];
    }
}