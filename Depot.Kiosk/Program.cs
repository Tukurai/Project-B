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

        var wijzigen = new Menu('2', "Reservering wijzigen", "Wijzig een reservering voor een rondleiding.", EditReservation);
        consoleMenu.AddMenuItem(wijzigen);

        var annuleren = new Menu('3', "Reservering annuleren", "Annuleer een reservering voor een rondleiding.", CancelReservation);
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

    private static void StartReservation()
    {
        int maxReservations = 13;
        var amountOfTickets = UserInput.GetNumber($"Hoeveel plaatsen wilt u reserveren? Voor elke plek wordt een ticketnummer gevraagd. (Maximaal {maxReservations})", 0, maxReservations);
        bool wachtlijst = false;
        bool retry = false;
        Tour? tour;

        do
        {
            tour = GetTour(amountOfTickets);
            if (tour == null)
            {
                Console.WriteLine("Druk op enter om terug naar het hoofdmenu te gaan.");
                consoleMenu?.Reset();
                Console.ReadLine();
                return;
            }

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

    private static Tour? GetTour(int amountOfTickets)
    {
        var today = DateTime.Now;
        var todaysTours = depotContext.Tours.Where(t => t.Start.DayOfYear == today.DayOfYear && t.Start.Year == today.Year).ToList();

        if (todaysTours.Count <= 0)
        {
            Console.WriteLine("Er zijn vandaag geen rondleidingen.");
            return null;
        }

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
}