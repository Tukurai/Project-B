using Depot.Common.Navigation;
using Depot.DAL;
using Depot.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Depot.Afdelingshoofd;

class Program
{
    private static Menu? consoleMenu;
    private static DepotContext depotContext = new DepotContext();

    static void Main(string[] args)
    {
        Console.WriteLine("Loading context data");
        depotContext.LoadJson();

        consoleMenu = new Menu("Afdelingshoofd", "Maak uw keuze uit het menu hieronder:");

        var afsluiten = new Menu('0', "Afsluiten", "Sluit het programma.", Close);
        consoleMenu.AddMenuItem(afsluiten);

        var rondleidingMaken = new Menu('1', "Rondleiding maken", "Rondleidingen aanmaken voor morgen.", CreateTours);
        consoleMenu.AddMenuItem(rondleidingMaken);

        var rondleidingBekijken = new Menu('2', "Rondleiding bekijken", "Rondleidingen bekijken.", ViewTours);
        consoleMenu.AddMenuItem(rondleidingBekijken);

        consoleMenu.Show();
    }

    private static void ViewTours()
    {
        var tours = depotContext.Tours.ToList();
        foreach (var tour in tours)
        {
            Console.WriteLine($"Rondleiding om {tour.Start}.");
        }
        Console.WriteLine("Druk op enter om terug naar het hoofdmenu te gaan.");
        Console.ReadLine();
    }

    private static void CreateTours()
    {
        Console.WriteLine("Hoe laat beginnen de rondleidingen morgen?");
        var beginTijd = GetTime();

        Console.WriteLine("Hoe laat eindigen de rondleidingen morgen?");
        var eindeTijd = GetTime();

        Console.WriteLine("Hoeveel minuten zit er tussen de rondleidingen?");
        var interval = GetInterval();

        var startTime = DateTime.Now.Date.AddDays(1).AddMilliseconds(beginTijd.TotalMilliseconds);
        var endTime = DateTime.Now.Date.AddDays(1).AddMilliseconds(eindeTijd.TotalMilliseconds);

        List<Tour> tours = new List<Tour>();
        for (var time = startTime; time < endTime; time = time.AddMinutes(interval))
        {
            tours.Add(new Tour { Start = time });
        }

        depotContext.Tours.AddRange(tours);
        foreach (var tour in tours)
        {
            Console.WriteLine($"Rondleiding aangemaakt voor {tour.Start}.");
        }
        depotContext.SaveChanges();

        Console.WriteLine("Druk op enter om terug naar het hoofdmenu te gaan.");
        consoleMenu?.Reset();
        Console.ReadLine();
    }

    private static TimeSpan GetTime()
    {
        do
        {
            string beginTijd = Console.ReadLine() ?? "";

            if (TimeSpan.TryParseExact(beginTijd, "h\\:m", null, out TimeSpan time))
            {
                return time;
            }
            else
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.WriteLine("Ongeldige tijd. Hou het formaat 'uren:minuten' aan.");
            }
        } while (true);
    }

    private static int GetInterval()
    {
        do
        {
            string intervalString = Console.ReadLine() ?? "";

            if (!int.TryParse(intervalString, out int interval) || interval < 1 || interval > 60)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.WriteLine("Ongeldige invoer.");
            }

            return interval;
        } while (true);
    }

    private static void Close()
    {
        if (consoleMenu != null)
        {
            consoleMenu.IsShowing = false;
        }
    }
}