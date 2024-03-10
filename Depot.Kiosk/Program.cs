using Depot.Common.Navigation;
using Depot.Common.Validation;
using Depot.DAL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Depot.Kiosk;

class Program
{
    private static Menu? consoleMenu;
    private static DepotContext depotContext = new DepotContext();
    private static int MaxReservations = 13;

    static void Main(string[] args)
    {
        Console.WriteLine("Loading context data");
        depotContext.LoadJson();

        consoleMenu = new Menu("Kiosk", "Maak uw keuze uit het menu hieronder:");

        var reserveren = new Menu('1', "Reservering maken", "Maak een reservering voor een rondleiding.", StartReservation);
        consoleMenu.AddMenuItem(reserveren);

        var wijzigen = new Menu('2', "Reservering wijzigen", "Wijzig een reservering voor een rondleiding.", EditReservation);
        consoleMenu.AddMenuItem(wijzigen);

        var annuleren = new Menu('3', "Reservering annuleren", "Annuleer een reservering voor een rondleiding.", CancelReservation);
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
        int tourId = GetTour(amountOfTickets);

        // GetTour returns 0 when user wants to return to the main menu.
        if (tourId == 0)
        {
            Console.WriteLine("Druk op enter om terug naar het hoofdmenu te gaan.");
            consoleMenu?.Reset();
            Console.ReadLine();
            return;
        }

        List<int> ticketNumbers = new List<int>();
        for (int i = 0; i < amountOfTickets; i++)
        {
            ticketNumbers.Add(UserInput.GetNumber("Wat is uw Ticketnummer?", min: 1));
        }

        Dictionary<int, int> ticketReservations = GetReservations(ticketNumbers);
        if (ticketReservations.Count == 0)
        {
            ReserveTour(tourId, ticketNumbers);
        }
        else
        {
            Console.WriteLine("1 of meerdere tickets zijn al geregistreerd bij een andere rondleiding: ");

            foreach (KeyValuePair<int, int> keyValue in ticketReservations)
            {
                Console.WriteLine($"Nr: {keyValue.Key} gereserveerd bij Tour: {keyValue.Value}");
            }

            Console.WriteLine("Annuleer eerst de reservering(en) om voor een nieuwe rondleiding in te schrijven.");
            Console.WriteLine("Druk op enter om terug naar het hoofdmenu te gaan.");
            consoleMenu?.Reset();
            Console.ReadLine();
        }
    }

    private static void EditReservation()
    {
        int ticketNumber = UserInput.GetNumber("Wat is uw Ticketnummer?", min: 1);

        var reservation = GetReservations(new List<int>() {ticketNumber});
        if (reservation.Count == 0)
        {
            Console.WriteLine("Geen reservering gevonden voor dit ticketnummer");
        }
        else
        {
            Console.WriteLine("Reservering gevonden: Rondleiding {poep} om {scoop uur}");
        }
    }

    private static void CancelReservation()
    {
        var ticketNumber = UserInput.GetNumber("Wat is uw Ticketnummer?", min: 1);

        CancelTour(ticketNumber);
    }

    private static Dictionary<int, int> GetReservations(List<int> ticketNumbers)
    {
        Dictionary<int, int> ticketRegistrations = new();

        foreach (var tour in depotContext.Tours)
        {
            foreach (int ticketNumber in ticketNumbers)
            {
                if (tour.Registrations.Contains(ticketNumber))
                {
                    ticketRegistrations[ticketNumber] = tour.Id;
                }
            }
        }

        return ticketRegistrations;
    }

    private static void ReserveTour(int tourId, List<int> ticketNumbers)
    {
        //Write tour reservation(s) to the JSON data
        depotContext.Tours.FirstOrDefault(tour => tour.Id == tourId)!.Registrations.AddRange(ticketNumbers);
        depotContext.SaveChanges();

        Console.WriteLine($"Uw reservering is geplaatst voor tour {tourId}, met {ticketNumbers.Count()} mensen.");
        Console.WriteLine("Druk op enter om terug naar het hoofdmenu te gaan.");
        consoleMenu?.Reset();
        Console.ReadLine();
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

    private static int GetTour(int amountOfTickets)
    {
        // var tours = depotContext.Tours.Where(q => q.Start.Date == DateTime.Now.AddDays(1).Date).ToList();
        var tours = depotContext.Tours.ToList();

        do
        {
            Console.WriteLine();
            foreach (var tour in tours)
            {
                string tourTime = tour.Start.ToString("HH:mm");

                if (amountOfTickets > MaxReservations - tour.Registrations.Count)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                Console.WriteLine($"{tour.Id}. {tourTime} - Vrije plekken: {MaxReservations - tour.Registrations.Count}");
                Console.ResetColor();
            }


            int tourNumber = UserInput.GetNumber("Welke rondleiding wilt u reserveren?", 1, tours.Count);

            if (amountOfTickets > MaxReservations - tours[tourNumber - 1].Registrations.Count)
            {
                Console.WriteLine("De rondleiding heeft niet genoeg vrije plekken om de inschrijving te voltooien.\n");
                int userInput = UserInput.GetNumber("Maak een keuze:\n1. Andere reservering kiezen\n2. Terug naar het hoofdmenu", 1, 2);

                if (userInput == 2)
                {
                    // Return 0 when user wants to return to the main menu
                    return 0;
                }
            }
            else
            {
                return tourNumber;
            }
        }
        while (true);
    }
}