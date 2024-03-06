using Depot.Common.Navigation;
using Depot.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json;

namespace Depot.Gids;

class Program
{
    private static Menu? consoleMenu;
    private static DepotContext depotContext = new DepotContext();

    static void Main(string[] args)
    {
        depotContext.LoadJson();
        Console.WriteLine("Hello, World! Please login to the Depot.");

        if (Login())
        {
            consoleMenu = new Menu("Gids", "Maak uw keuze uit de rondleidingen hieronder: ");

            var afsluiten = new SubMenu('0', "Afsluiten", "Sluit het programma.", Close);
            consoleMenu.AddMenuItem(afsluiten);

            var bekijken1 = new SubMenu('1', "Rondleidingen bekijken", "Rondleiding 1 bekijken", GetTour);
            consoleMenu.AddMenuItem(bekijken1);

            consoleMenu.Show();
        }
    }

    private static void Close()
    {
        if (consoleMenu != null)
        {
            consoleMenu.IsShowing = false;
        }
    }

    private static bool Login()
    {
        Console.WriteLine("Enter your barcode to log in: ");
        do
        {
            string enteredBarcode = Console.ReadLine() ?? "";

            if (!int.TryParse(enteredBarcode, out int userId) || userId < 1)
            {
                if (!depotContext.Users.Where(u => u.Id == userId).Any())
                {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.WriteLine("Ongeldige invoer.");
                }    
            }
        } while (true);

        return true;
    }

    private static void GetTour(int tourNr)
    {
        var tour = depotContext.Tours.Where(t => t.Id == tourNr).FirstOrDefault();

        Console.WriteLine($"Rondleiding {tour.Id}: {tour.Start}");
        Console.WriteLine("Alle geregistreerde bezoekers voor deze rondleiding: ");
        foreach (var ticketnummer in tour.Registrations)
        {
            Console.WriteLine($"Bezoeker {ticketnummer}");
        }

        Console.WriteLine();
        Console.WriteLine("Alle tickets in de wachtrij voor deze rondleiding: ");
        foreach (var ticketnummer in tour.Queue)
        {
            Console.WriteLine($"Bezoeker {ticketnummer}");
        }

        // Andere opties toevoegen, verwijderen, inchecken
        Console.WriteLine("Druk op enter om terug naar het hoofdmenu te gaan.");
        consoleMenu?.Reset();
        Console.ReadLine();
    }
}