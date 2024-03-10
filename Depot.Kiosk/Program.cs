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

        var bekijken = new Menu('3', "Reservering bekijken", "Bekijk uw reservering.", ViewReservation);
        consoleMenu.AddMenuItem(bekijken);

        consoleMenu.Show();
    }

    private static void ViewReservation()
    {
        var ticketNumber = UserInput.GetNumber("Wat is uw Ticketnummer?", min: 1);
        var reservation = depotContext.Tours.FirstOrDefault(t => t.Registrations.Contains(ticketNumber));

        if (reservation != null)
        {
            Console.WriteLine($"Uw reservering voor rondleiding ({reservation.Id}) van ({reservation.Start}) is gevonden.");
        }
        else
        {
            Console.WriteLine("Geen reservering gevonden voor het opgegeven ticketnummer.");
        }

        var queue = depotContext.Tours.FirstOrDefault(t => t.Queue.Contains(ticketNumber));

        if (queue != null)
        {
            Console.WriteLine($"U staat op de wachtlijst voor rondleiding ({queue.Id}) van ({queue.Start}).");
        }

        Console.WriteLine("Druk op enter om terug naar het hoofdmenu te gaan.");
        consoleMenu?.Reset();
        Console.ReadLine();
    }

    private static void StartReservation()
    {
        int maxReservations = 13;
        var amountOfTickets = UserInput.GetNumber($"Hoeveel plaatsen wilt u reserveren? Voor elke plek wordt een ticketnummer gevraagd. (Maximaal {maxReservations})", 0, maxReservations);
        bool wachtlijst = false;
        bool retry = false;
        Tour tour;

        do
        {
            tour = GetTour(amountOfTickets);

            if (maxReservations - (tour.Registrations.Count + amountOfTickets) < 0)
            {
                Console.WriteLine("De rondleiding heeft niet genoeg vrije plekken om de inschrijving te voltooien.");
                Console.WriteLine("Dit zijn uw opties:");
                Console.WriteLine("0. Terug naar het hoofdmenu.");
                Console.WriteLine("1. Andere reservering kiezen.");
                Console.WriteLine("2. Aanmelden op de wachtlijst.");

                int userInput = UserInput.GetNumber("Maak uw keuze:", 0, 2);

                switch (userInput)
                {
                    case 0:
                        consoleMenu?.Reset();
                        Console.ReadLine();
                        return;
                    case 1:
                        retry = true;
                        break;
                    case 2:
                        wachtlijst = true;
                        break;
                }
            }
        } while (retry);

        List<int> ticketNumbers = new List<int>();
        for (int i = 0; i < amountOfTickets; i++)
        {
            ticketNumbers.Add(UserInput.GetNumber("Wat is uw Ticketnummer?", min: 1));
        }

        if (wachtlijst)
        {
            tour.Queue.AddRange(ticketNumbers);
            Console.WriteLine($"U staat op de wachtlijst voor tour {tour.Id}, met {ticketNumbers.Count()} mensen.");
        }
        else
        {
            tour.Registrations.AddRange(ticketNumbers);
            Console.WriteLine($"Uw reservering is geplaatst voor tour {tour.Id}, met {ticketNumbers.Count()} mensen.");
        }

        Console.WriteLine("Druk op enter om terug naar het hoofdmenu te gaan.");
        consoleMenu?.Reset();
        Console.ReadLine();
    }

    private static void CancelReservation()
    {
        var ticketNumber = UserInput.GetNumber("Wat is uw Ticketnummer?", min: 1);
        var reservation = depotContext.Tours.FirstOrDefault(t => t.Registrations.Contains(ticketNumber));

        if (reservation != null)
        {
            Console.WriteLine($"Reservering voor rondleiding ({reservation.Id}) van ({reservation.Start}) gevonden.");
            Console.WriteLine("Weet u zeker dat u uw reservering wil annuleren? (y/n)");

            var userInput = Console.ReadLine() ?? "";
            if (userInput == "y")
            {
                reservation.Registrations.Remove(ticketNumber);
                depotContext.SaveChanges();
                Console.WriteLine($"Uw reservering voor ({reservation.Id}) van ({reservation.Start}) is geannuleerd.");
            }
            else
            {
                Console.WriteLine("Reservering niet geannuleerd.");
            }
        }
        else
        {
            Console.WriteLine("Geen reservering gevonden voor het opgegeven ticketnummer.");
        }

        var queue = depotContext.Tours.FirstOrDefault(t => t.Queue.Contains(ticketNumber));

        if (queue != null)
        {
            Console.WriteLine($"U staat op de wachtlijst voor rondleiding ({queue.Id}) van ({queue.Start}).");
            Console.WriteLine("Weet u zeker dat u uw wachtlijst reservering wil annuleren? (y/n)");

            var userInput = Console.ReadLine() ?? "";
            if (userInput == "y")
            {
                queue.Queue.Remove(ticketNumber);
                depotContext.SaveChanges();
                Console.WriteLine($"Uw wachtlijst reservering voor ({queue.Id}) van ({queue.Start}) is geannuleerd.");
            }
            else
            {
                Console.WriteLine("Wachtlijst reservering niet geannuleerd.");
            }
        }

        Console.WriteLine("Druk op enter om terug naar het hoofdmenu te gaan.");
        consoleMenu?.Reset();
        Console.ReadLine();
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
            var ruimteMessage = ruimte ? "Voldoende ruimte" : "Onvoldoende ruimte";

            Console.ForegroundColor = ruimte ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"{i} Rondleiding {todaysTours[i].Id}: {todaysTours[i].Start}. ({vrijePlekken} plekken vrij) ({ruimteMessage})");
            Console.ResetColor();
        }

        int tourIndex = UserInput.GetNumber("Welke rondleiding wilt u reserveren?", 0, todaysTours.Count - 1);
        var tour = todaysTours[tourIndex];

        return todaysTours[tourIndex];
    }


    //private static int GetTour(int amountOfTickets)
    //{
    //    // var tours = depotContext.Tours.Where(q => q.Start.Date == DateTime.Now.AddDays(1).Date).ToList();
    //    var tours = depotContext.Tours.ToList();

    //    do
    //    {
    //        Console.WriteLine();
    //        foreach (var tour in tours)
    //        {
    //            string tourTime = tour.Start.ToString("HH:mm");

    //            if (amountOfTickets > MaxReservations - tour.Registrations.Count)
    //            {
    //                Console.ForegroundColor = ConsoleColor.Red;
    //            }
    //            else
    //            {
    //                Console.ForegroundColor = ConsoleColor.Green;
    //            }

    //            Console.WriteLine($"{tour.Id}. {tourTime} - Vrije plekken: {MaxReservations - tour.Registrations.Count}");
    //            Console.ResetColor();
    //        }


    //        int tourNumber = UserInput.GetNumber("Welke rondleiding wilt u reserveren?", 1, tours.Count);

    //        if (amountOfTickets > MaxReservations - tours[tourNumber - 1].Registrations.Count)
    //        {
    //            Console.WriteLine("De rondleiding heeft niet genoeg vrije plekken om de inschrijving te voltooien.\n");
    //            int userInput = UserInput.GetNumber("Maak een keuze:\n1. Andere reservering kiezen\n2. Terug naar het hoofdmenu", 1, 2);

    //            if (userInput == 2)
    //            {
    //                // Return 0 when user wants to return to the main menu
    //                return 0;
    //            }
    //        }
    //        else
    //        {
    //            return tourNumber;
    //        }
    //    }
    //    while (true);
    //}
}