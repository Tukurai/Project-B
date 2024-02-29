using Depot.Common.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Depot.Kiosk;

class Program
{
    private static Menu? consoleMenu;

    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        consoleMenu = new Menu("Kiosk", "Maak uw keuze uit het menu hieronder:");

        var afsluiten = new SubMenu('0', "Afsluiten", "Sluit het programma.", Close);
        consoleMenu.AddMenuItem(afsluiten);

        var reserveren = new SubMenu('1', "Reservering maken", "Maak een reservering voor een rondleiding.", StartReservation);
        consoleMenu.AddMenuItem(reserveren);

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
        var amountOfTickets = GetAmountOfTickets();
        var tourId = GetTour();
        
        List<int> ticketNumbers = new List<int>();
        for (int i = 0; i < amountOfTickets; i++)
        {
            ticketNumbers.Add(GetTicketNumber());
        }

        ReserveTour(tourId, ticketNumbers);
    }

    private static void ReserveTour(int tourId, List<int> ticketNumbers)
    {
        Console.WriteLine($"Uw reservering is geplaatst voor tour {tourId}, met {ticketNumbers.Count()} mensen.");
        Console.WriteLine("Druk op enter om terug naar het hoofdmenu te gaan.");
        consoleMenu?.Reset();
        Console.ReadLine();
    }

    private static int GetTour()
    {
        Console.WriteLine("Welke rondleiding wilt u reserveren?");
        Console.WriteLine("1. Rondleiding 9:00");
        Console.WriteLine("2. Rondleiding 10:00");
        Console.WriteLine("3. Rondleiding 11:00");
        Console.WriteLine("4. Rondleiding 12:00");
        Console.WriteLine("5. Rondleiding 13:00");
        Console.WriteLine("6. Rondleiding 14:00");
        Console.WriteLine("7. Rondleiding 15:00");
        Console.WriteLine("8. Rondleiding 16:00");
        Console.WriteLine("9. Rondleiding 17:00");
        Console.WriteLine("10. Rondleiding 18:00");

        do
        {
            string tourString = Console.ReadLine() ?? "";

            if (!int.TryParse(tourString, out int tourId) || tourId > 10 || tourId < 1)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.WriteLine("Ongeldige invoer.");
            }

            return tourId;
        } while (true);
    }

    private static int GetAmountOfTickets()
    {
        Console.WriteLine("Hoeveel plaatsen wilt u reserveren? Voor elke plek wordt een ticketnummer gevraagd. (Maximaal 13)");
        do
        {
            string amountString = Console.ReadLine() ?? "";

            if (!int.TryParse(amountString, out int amount) || amount > 13 || amount < 1)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.WriteLine("Ongeldige invoer.");
            }

            return amount;
        } while (true);
    }

    private static int GetTicketNumber()
    {
        Console.WriteLine("Wat is uw Ticketnummer?");
        do
        {
            string ticketString = Console.ReadLine() ?? "";

            if (!int.TryParse(ticketString, out int ticketNumber) || ticketNumber < 1)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.WriteLine("Ongeldige invoer.");
            }

            return ticketNumber;
        } while (true);
    }
}