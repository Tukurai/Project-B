using Depot.Common;
using Depot.Common.Navigation;
using Depot.Common.Validation;
using Depot.Common.Workflow;
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
        depotContext.LoadContext();

        while (true)
        {
            var userId = UserInput.GetNumber(Localization.Scan_uw_pas, 1);
            var validation = new UserValidationFlow(depotContext, userId, Role.Afdelingshoofd);
            var message = validation.Validate(out bool valid);

            Console.WriteLine(message);
            if (valid)
            {
                consoleMenu = new Menu($"{(Role)validation.User!.Role} {validation.User!.Name}", Localization.Maak_uw_keuze);

                var rondleidingen = new Menu('1', Localization.Rondleidingen, Localization.Rondleidingen_beheren);
                rondleidingen.AddMenuItem(new Menu('1', Localization.Vandaag_plannen, Localization.Rondleidingen_aanmaken_voor_vandaag, () => { CreateTours(0); }));
                rondleidingen.AddMenuItem(new Menu('2', Localization.Morgen_plannen, Localization.Rondleidingen_aanmaken_voor_morgen, () => { CreateTours(1); }));
                rondleidingen.AddMenuItem(new Menu('3', Localization.Plannen_tot_datum, Localization.Rondleidingen_aanmaken_tot_datum, CreateToursBetweenDate));
                rondleidingen.AddMenuItem(new Menu('4', Localization.Bekijken, Localization.Rondleidingen_bekijken, ViewTours));
                consoleMenu.AddMenuItem(rondleidingen);

                var gebruikers = new Menu('2', Localization.Gebruikers, Localization.Gebruikers_beheren);
                gebruikers.AddMenuItem(new Menu('1', Localization.Aanmaken, Localization.Gebruikers_aanmaken, CreateUsers));
                gebruikers.AddMenuItem(new Menu('2', Localization.Bekijken, Localization.Gebruikers_Bekijken, ViewUsers));
                consoleMenu.AddMenuItem(gebruikers);

                consoleMenu.Show();
            }

            Console.WriteLine(Localization.Ongeldige_invoer);
            ResetMenuState();
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
        var createUsers = new CreateUsersFlow(depotContext);
        var amount = UserInput.GetNumber(Localization.Hoeveel_gebruikers_wilt_u_aanmaken, 1, 10);

        if (!createUsers.SetUserAmount((int?)amount))
        {
            ResetMenuState();
            return;
        }

        for (int i = 0; i < createUsers.Amount; i++)
        {
            Console.WriteLine($"{Localization.Welke_naam_krijgt_gebruiker} {i + 1}?");
            var name = Console.ReadLine() ?? "";
            var roleId = UserInput.GetNumber($"{Localization.Welke_rol} (0 = {Role.Bezoeker}, 1 = {Role.Gids}, 2 = {Role.Afdelingshoofd}", 0, 2);
            if (roleId == null)
            {
                break;
            }

            createUsers.AddUser(new User { Name = name, Role = (int)roleId.Value });
        }

        Console.WriteLine(createUsers.Validate(out _));
        ResetMenuState();
    }

    private static void ViewTours()
    {
        var today = DateTime.Now;

        var todaysTours = depotContext.Tours.Where(t => t.Start > today)
            .OrderBy(q => q.Start).Take(5).ToList();
        foreach (var tour in todaysTours)
        {
            Console.WriteLine($"{Localization.Rondleiding_om} {tour.Start.ToString("HH:mm")}.");
        }

        ResetMenuState();
    }

    private static void CreateToursBetweenDate()
    {
        var createTour = new CreateToursFlow(depotContext);

        if (!createTour.SetBeginDate(UserInput.GetDate(Localization.Start_datum_rondleidingen)) ||
            !createTour.SetEndDate(UserInput.GetDate(Localization.Eind_datum_rondleidingen)) ||
            !createTour.SetBeginTime(UserInput.GetTime(Localization.Start_tijd_rondleidingen)) ||
            !createTour.SetEndTime(UserInput.GetTime(Localization.Eind_tijd_rondleidingen)) ||
            !createTour.SetInterval((int?)UserInput.GetNumber(Localization.Minuten_tussen_rondleidingen, 1, 60)))
        {
            ResetMenuState();
            return;
        }

        Console.WriteLine(createTour.Validate(out _));
        ResetMenuState();
    }

    private static void CreateTours(int daysInTheFuture)
    {
        var createTour = new CreateToursFlow(depotContext);

        if (!createTour.SetBeginAndEndDateByDays(daysInTheFuture) ||
            !createTour.SetBeginTime(UserInput.GetTime(Localization.Start_tijd_rondleidingen)) ||
            !createTour.SetEndTime(UserInput.GetTime(Localization.Eind_tijd_rondleidingen)) ||
            !createTour.SetInterval((int?)UserInput.GetNumber(Localization.Minuten_tussen_rondleidingen, 1, 60)))
        {
            ResetMenuState();
            return;
        }

        Console.WriteLine(createTour.Validate(out _));
        ResetMenuState();
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