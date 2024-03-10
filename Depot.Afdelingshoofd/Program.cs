using Depot.Common.Navigation;
using Depot.Common.Validation;
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

        var rondleidingMaken = new Menu('1', "Rondleiding maken", "Rondleidingen aanmaken voor morgen.", CreateTours);
        consoleMenu.AddMenuItem(rondleidingMaken);

        var rondleidingBekijken = new Menu('2', "Rondleiding bekijken", "Rondleidingen bekijken.", ViewTours);
        consoleMenu.AddMenuItem(rondleidingBekijken);

        var gebruikerMaken = new Menu('3', "Gebruikers maken", "Gebruikers aanmaken.", CreateUsers);
        consoleMenu.AddMenuItem(gebruikerMaken);

        var gebruikerBekijken = new Menu('4', "Gebruikers bekijken", "Alle gebruikers bekijken.", ViewUsers);
        consoleMenu.AddMenuItem(gebruikerBekijken);

        consoleMenu.Show();
    }

    private static void ViewUsers()
    {
        var users = depotContext.Users.ToList();
        foreach (var user in users)
        {
            Console.WriteLine($"Gebruiker Id: {user.Id}, {user.Name}.");
        }
        Console.WriteLine("Druk op enter om terug naar het hoofdmenu te gaan.");
        Console.ReadLine();
    }

    private static void CreateUsers()
    {
        Console.WriteLine();
        var amount = UserInput.GetNumber("Hoeveel gebruikers wilt u aanmaken? (Max 10)", 1, 10);

        List<User> users = new List<User>();
        for (int i = 0; i < amount; i++)
        {
            Console.WriteLine($"Geef de naam op van gebruiker {i + 1}:");
            var name = Console.ReadLine() ?? "";
            users.Add(new User { Name = name });
        }

        depotContext.Users.AddRange(users);
        depotContext.SaveChanges();

        foreach (var user in users)
        {
            Console.WriteLine($"Gebruiker aangemaakt met Id: {user.Id}, {user.Name}.");
        }

        Console.WriteLine("Druk op enter om terug naar het hoofdmenu te gaan.");
        consoleMenu?.Reset();
        Console.ReadLine();
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
        var beginTijd = UserInput.GetTime("Hoe laat beginnen de rondleidingen morgen?");

        var eindeTijd = UserInput.GetTime("Hoe laat eindigen de rondleidingen morgen?");

        var interval = UserInput.GetNumber("Hoeveel minuten zit er tussen de rondleidingen?", 1, 60);

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
}