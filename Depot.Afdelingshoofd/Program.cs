using Depot.Common;
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
        Console.WriteLine(Localization.Load_context);
        depotContext.LoadJson();

        if (GetAccount(out User? user, new List<Role> { Role.Afdelingshoofd }))
        {
            consoleMenu = new Menu($"{user.Role} {user.Name}", Localization.Maak_uw_keuze);

            var rondleidingen = new Menu('1', Localization.Rondleidingen, Localization.Rondleidingen_beheren);
            rondleidingen.AddMenuItem(new Menu('1', Localization.Vandaag_plannen, Localization.Rondleidingen_aanmaken_voor_vandaag, () => { CreateTours(0); }));
            rondleidingen.AddMenuItem(new Menu('2', Localization.Morgen_plannen, Localization.Rondleidingen_aanmaken_voor_morgen, () => { CreateTours(1); }));
            rondleidingen.AddMenuItem(new Menu('3', Localization.Bekijken, Localization.Rondleidingen_bekijken, ViewTours));
            consoleMenu.AddMenuItem(rondleidingen);

            var gebruikers = new Menu('2', Localization.Gebruikers, Localization.Gebruikers_beheren);
            gebruikers.AddMenuItem(new Menu('1', Localization.Aanmaken, Localization.Gebruikers_aanmaken, CreateUsers));
            gebruikers.AddMenuItem(new Menu('2', Localization.Bekijken, Localization.Gebruikers_Bekijken, ViewUsers));
            consoleMenu.AddMenuItem(gebruikers);

            consoleMenu.Show();
        }
    }

    private static void ViewUsers()
    {
        var users = depotContext.Users.ToList();
        foreach (var user in users)
        {
            Console.WriteLine($"{user.Role}, {user.Id}, {user.Name}.");
        }

        ResetMenuState();
    }

    private static void CreateUsers()
    {
        Console.WriteLine();
        var amount = UserInput.GetNumber(Localization.Hoeveel_gebruikers_wilt_u_aanmaken, 1, 10);
        if (amount == null)
        {
            ResetMenuState();
            return;
        }

        List<User> users = new List<User>();
        for (int i = 0; i < amount; i++)
        {
            Console.WriteLine($"{Localization.Welke_naam_krijgt_gebruiker} {i + 1}?");
            var name = Console.ReadLine() ?? "";
            var roleId = UserInput.GetNumber($"{Localization.Welke_rol} (0 = {Role.Bezoeker}, 1 = {Role.Gids}, 2 = {Role.Afdelingshoofd}", 0, 2);
            if (roleId == null)
            {
                break;
            }

            users.Add(new User { Name = name, Role = (Role)roleId.Value });
        }

        depotContext.Users.AddRange(users);
        depotContext.SaveChanges();

        foreach (var user in users)
        {
            Console.WriteLine($"{Localization.Aangemaakt}: {user.Role}, {user.Id}, {user.Name}.");
        }

        ResetMenuState();
    }

    private static void ViewTours()
    {

        var today = DateTime.Now;
        var todaysTours = depotContext.Tours.Where(t =>
            t.Start.DayOfYear == today.DayOfYear &&
            t.Start.Year == today.Year &&
            t.Start.TimeOfDay > today.TimeOfDay)
            .OrderBy(q => q.Start).ToList();
        foreach (var tour in todaysTours)
        {
            Console.WriteLine($"{Localization.Rondleiding_om} {tour.Start.ToString("HH:mm")}.");
        }
        Console.WriteLine(Localization.Ga_terug);
        Console.ReadLine();
    }

    private static void CreateTours(int daysInTheFuture)
    {
        var beginTijd = UserInput.GetTime(Localization.Start_tijd_rondleidingen);
        if (beginTijd == null) {
            ResetMenuState();
            return; 
        }

        var eindeTijd = UserInput.GetTime(Localization.Eind_tijd_rondleidingen);
        if (eindeTijd == null)
        {
            ResetMenuState();
            return;
        }

        var interval = UserInput.GetNumber(Localization.Minuten_tussen_rondleidingen, 1, 60);
        if (interval == null)
        {
            ResetMenuState();
            return;
        }

        var startTime = DateTime.Now.Date.AddDays(daysInTheFuture).AddMilliseconds(beginTijd.Value.TotalMilliseconds);
        var endTime = DateTime.Now.Date.AddDays(daysInTheFuture).AddMilliseconds(eindeTijd.Value.TotalMilliseconds);

        List<Tour> tours = new List<Tour>();
        for (var time = startTime; time.AddMinutes(Globals.Rondleiding_Duur) < endTime; time = time.AddMinutes(interval.Value))
        {
            tours.Add(new Tour { Start = time });
        }

        depotContext.Tours.AddRange(tours);
        depotContext.SaveChanges();

        foreach (var tour in tours)
        {
            Console.WriteLine($"{Localization.Rondleidingen_aangemaakt_voor} {tour.Start}.");
        }

        Console.WriteLine(Localization.Ga_terug);
        consoleMenu?.Reset();
        Console.ReadLine();
    }

    private static bool GetAccount(out User? user, List<Role> allowedRoles)
    {
        while (true)
        {
            var userId = UserInput.GetNumber(Localization.Scan_uw_pas, 1);
            if (userId == null)
            {
                ResetMenuState();
                user = null;
                return false;
            }

            user = depotContext.Users.Where(u => u.Id == userId).FirstOrDefault();
            if (user != null && allowedRoles.Contains(user.Role))
            {
                return true;
            }

            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine(Localization.Ongeldige_invoer);
        }
    }

    public static void ResetMenuState()
    {
        if (consoleMenu != null)
        {
            Console.WriteLine(Localization.Ga_terug);
            consoleMenu.Reset();
            Console.ReadLine();
        }
    }
}