using Depot.Common;
using Depot.Common.Navigation;
using Depot.Common.Validation;
using Depot.DAL;
using Depot.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Depot.Gids;

class Program
{
    private static PagingMenu? consoleMenu;
    private static DepotContext depotContext = new DepotContext();

    static void Main(string[] args)
    {
        Console.WriteLine("Loading context data");
        depotContext.LoadJson();

        if (GetAccount(out User? user))
        {
            consoleMenu = new PagingMenu($"Gids ({user.Name})", "Maak uw keuze uit de rondleidingen hieronder: ");
            consoleMenu.SetListItems(CreateTourList());

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
    private static void RemoveVisitor(int tourId)
    {
        int ticketNummer = UserInput.GetNumber("Wat is het ticketnummer van de bezoeker?", 1);
        var tour = depotContext.Tours.Where(t => t.Id == tourId).FirstOrDefault();
        if (tour != null)
        {
            if (tour.Registrations.Contains(ticketNummer))
            {
                tour.Registrations.Remove(ticketNummer);
                depotContext.SaveChanges();
                Console.WriteLine("Bezoeker verwijderd van de rondleiding.");
                Console.WriteLine($"Druk op enter om terug te gaan.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Deze bezoeker is niet geregistreerd voor deze rondleiding.");
            Console.WriteLine($"Druk op enter om terug te gaan.");
            Console.ReadLine();
        }
    }
    private static void AddVisitor(int tourId)
    {
        int ticketNummer = UserInput.GetNumber("Wat is het ticketnummer van de bezoeker?", 1);
        var tour = depotContext.Tours.Where(t => t.Id == tourId).FirstOrDefault();
        if (tour != null)
        {
            if (tour.Registrations.Contains(ticketNummer))
            {
                Console.WriteLine("Deze bezoeker is al geregistreerd voor deze rondleiding.");
                Console.WriteLine($"Druk op enter om terug te gaan.");
                Console.ReadLine();
                return;
            }

            if (tour.Queue.Contains(ticketNummer))
            {
                Console.WriteLine("Deze bezoeker stond op de wachtlijst en is daar verwijderd.");
                tour.Queue.Remove(ticketNummer);
            }

            tour.Registrations.Add(ticketNummer);
            depotContext.SaveChanges();
            Console.WriteLine("Bezoeker toegevoegd aan de rondleiding.");
            Console.WriteLine($"Druk op enter om terug te gaan.");
            Console.ReadLine();
        }
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

    private static bool GetAccount(out User? user)
    {
        do
        {
            var userId = UserInput.GetNumber("Wat is uw gebruiker ID?", 1);

            user = depotContext.Users.Where(u => u.Id == userId).FirstOrDefault();
            if (user != null)
            {
                return true;
            }

            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine("Ongeldige invoer.");
        } while (true);
    }
}