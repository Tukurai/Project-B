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
        Console.WriteLine(Localization.Load_context);
        depotContext.LoadJson();

        if (GetAccount(out User? user, new List<Role> { Role.Gids, Role.Afdelingshoofd }))
        {
            consoleMenu = new PagingMenu($"{user.Role} {user.Name}", Localization.Maak_uw_keuze);
            consoleMenu.SetListItems(CreateTourList());

            consoleMenu.Show();
        }
    }

    private static List<Menu> CreateTourList()
    {
        var resultTours = new List<Menu>();
        var index = 3;

        var today = DateTime.Now;
        var todaysTours = depotContext.Tours.Where(t =>
            t.Start.DayOfYear == today.DayOfYear &&
            t.Start.Year == today.Year &&
            t.Start.TimeOfDay > today.TimeOfDay)
            .OrderBy(q => q.Start).ToList();

        todaysTours.ForEach(t =>
        {
            var tourDetailsMenuItem = new Menu(index.ToString()[0], $"{Localization.Rondleidingen_van} {t.Start.ToString("HH:mm")}", $"{Localization.Aanmeldingen}: {t.RegisteredTickets.Count}, {Localization.Gestart}: {(t.Departed ? Localization.Ja : Localization.Nee)}", null, consoleMenu);
            tourDetailsMenuItem.AddMenuItem(new Menu('1', Localization.Bekijk_details, Localization.Bekijk_details_van_deze_rondleiding, () => { GetTour(t.Id); }, tourDetailsMenuItem));
            tourDetailsMenuItem.AddMenuItem(new Menu('2', Localization.Toevoegen_bezoeker, Localization.Een_bezoeker_toevoegen_aan_deze_rondleiding, () => { AddVisitor(t.Id); }, tourDetailsMenuItem));
            tourDetailsMenuItem.AddMenuItem(new Menu('3', Localization.Verwijderen_bezoeker, Localization.Een_bezoeker_verwijderen_van_deze_rondleiding, () => { RemoveVisitor(t.Id); }, tourDetailsMenuItem));
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
        int ticketNummer = UserInput.GetNumber(Localization.Scan_uw_ticket, 1);
        var tour = depotContext.Tours.Where(t => t.Id == tourId).FirstOrDefault();
        if (tour != null)
        {
            if (tour.RegisteredTickets.Contains(ticketNummer))
            {
                tour.RegisteredTickets.Remove(ticketNummer);
                depotContext.SaveChanges();
                Console.WriteLine(Localization.Ticket_verwijderd);
                Console.WriteLine(Localization.Ga_terug);
                Console.ReadLine();
                return;
            }

            Console.WriteLine(Localization.Aanmelding_niet_gevonden);
            Console.WriteLine(Localization.Ga_terug);
            Console.ReadLine();
        }
    }
    private static void AddVisitor(int tourId)
    {
        int ticketNummer = UserInput.GetNumber(Localization.Scan_uw_ticket, 1);
        var tour = depotContext.Tours.Where(t => t.Id == tourId).FirstOrDefault();
        if (tour != null)
        {
            if (tour.RegisteredTickets.Contains(ticketNummer))
            {
                Console.WriteLine(Localization.Ticket_al_geregistreerd);
                Console.WriteLine(Localization.Ga_terug);
                Console.ReadLine();
                return;
            }

            tour.RegisteredTickets.Add(ticketNummer);
            depotContext.SaveChanges();
            Console.WriteLine(Localization.Ticket_toegevoegd);
            Console.WriteLine(Localization.Ga_terug);
            Console.ReadLine();
        }
    }

    private static void GetTour(int tourNr)
    {
        var tour = depotContext.Tours.Where(t => t.Id == tourNr).FirstOrDefault();

        Console.WriteLine($"{Localization.Rondleiding_om} {tour?.Start.ToString("HH:mm")}");
        Console.WriteLine(Localization.Alle_ticketnummers);
        foreach (var ticketNummer in tour?.RegisteredTickets)
        {
            Console.WriteLine($"{Localization.Ticket} {ticketNummer}");
        }

        Console.WriteLine(Localization.Ga_terug);
        Console.ReadLine();
    }

    private static bool GetAccount(out User? user, List<Role> allowedRoles)
    {
        do
        {
            var userId = UserInput.GetNumber(Localization.Scan_uw_pas, 1);

            user = depotContext.Users.Where(u => u.Id == userId).FirstOrDefault();
            if (user != null && allowedRoles.Contains(user.Role))
            {
                return true;
            }

            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine(Localization.Ongeldige_invoer);
        } while (true);
    }
}