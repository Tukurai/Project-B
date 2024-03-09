using Depot.Common.Navigation;
using Depot.DAL;
using Depot.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Depot.Gids;

class Program
{
    private static Menu? consoleMenu;
    private static DepotContext depotContext = new DepotContext();

    static void Main(string[] args)
    {
        Console.WriteLine("Loading context data");
        depotContext.LoadJson();

        Console.WriteLine("Please login to the Depot:");

        if (true) //(Login())
        { 
            consoleMenu = new PagingMenu("Gids", "Maak uw keuze uit de rondleidingen hieronder: ", CreateTourList());

            consoleMenu.Show();
        }
    }

    private static List<Menu> CreateTourList()
    {
        var resultTours = new List<Menu>();
        var index = 3;

        depotContext.Tours.ToList().ForEach(t =>
        {
            var tourDetailsMenuItem = new Menu(index.ToString()[0], $"Tour {t.Id}", $"{t.Start}, reserveringen: {t.Registrations.Count}, wachtlijst {t.Queue.Count}", null, consoleMenu);
            tourDetailsMenuItem.AddMenuItem(new Menu('1', "Bekijk details", "Bekijk details van deze rondleiding.", () => { GetTour(t.Id); }, tourDetailsMenuItem));
            tourDetailsMenuItem.AddMenuItem(new Menu('2', "Toevoegen bezoeker", "Een bezoeker toevoegen aan deze rondleiding.", () => { AddVisitor(t.Id); }, tourDetailsMenuItem));
            tourDetailsMenuItem.AddMenuItem(new Menu('3', "Verwijderen bezoeker", "Een bezoeker verwijderen van deze rondleiding.", () => { RemoveVisitor(t.Id); }, tourDetailsMenuItem));

            resultTours.Add(tourDetailsMenuItem);
            index++;
            if (index == 10)
            {
                index = 3;
            }
        });

        return resultTours;
    }

    private static void RemoveVisitor(int tourId)
    {
        throw new NotImplementedException();
    }

    private static void AddVisitor(int tourId)
    {
        throw new NotImplementedException();
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
                var user = depotContext.Users.Where(u => u.Id == userId).FirstOrDefault();
                if (user != null)
                { 
                    Console.WriteLine($"Welkom {user.Name}.");
                    return true;
                }

                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.WriteLine("Ongeldige invoer.");
            }
        } while (true);
    }

    private static void GetTour(int tourNr)
    {
        var tour = depotContext.Tours.Where(t => t.Id == tourNr).FirstOrDefault();

        Console.WriteLine($"Rondleiding {tour?.Id}: {tour?.Start}");
        Console.WriteLine("Alle geregistreerde bezoekers voor deze rondleiding: ");
        foreach (var ticketnummer in tour?.Registrations)
        {
            Console.WriteLine($"Bezoeker {ticketnummer}");
        }

        Console.WriteLine();
        Console.WriteLine("Alle tickets in de wachtrij voor deze rondleiding: ");
        foreach (var ticketnummer in tour.Queue)
        {
            Console.WriteLine($"Bezoeker {ticketnummer}");
        }
        Console.WriteLine($"Druk op enter om terug te gaan.");
        Console.ReadLine();
    }
}