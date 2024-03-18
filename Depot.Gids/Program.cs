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
        depotContext.LoadContext();

        if (GetAccount(out User? user, new List<Role> { Role.Gids, Role.Afdelingshoofd }))
        {
            consoleMenu = new PagingMenu($"{user!.Role} {user!.Name}", Localization.Maak_uw_keuze);
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
            var tourDetailsMenuItem = new Menu(index.ToString()[0], $"{Localization.Rondleidingen_van} {t.Start.ToString("HH:mm")}", $"{Localization.Aanmeldingen}: {t.RegisteredTickets.Count}/{Globals.Maximum_Plekken}, {Localization.Gestart}: {(t.Departed ? Localization.Ja : Localization.Nee)}", null, consoleMenu);
            tourDetailsMenuItem.AddMenuItem(new Menu('1', Localization.Bekijk_details, Localization.Bekijk_details_van_deze_rondleiding, () => { GetTour(t.Id); }, tourDetailsMenuItem));
            tourDetailsMenuItem.AddMenuItem(new Menu('2', Localization.Toevoegen_bezoeker, Localization.Een_bezoeker_toevoegen_aan_deze_rondleiding, () => { AddVisitor(t.Id); }, tourDetailsMenuItem));
            tourDetailsMenuItem.AddMenuItem(new Menu('3', Localization.Verwijderen_bezoeker, Localization.Een_bezoeker_verwijderen_van_deze_rondleiding, () => { RemoveVisitor(t.Id); }, tourDetailsMenuItem));
            tourDetailsMenuItem.AddMenuItem(new Menu('4', Localization.Start,Localization.Start_Rondleiding, () => { StartTour(t.Id); }, tourDetailsMenuItem));
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

    private static void StartTour(long tourId)
    {
        var tour = depotContext.Tours.FirstOrDefault(t => t.Id == tourId);
        int maxSpots = Globals.Maximum_Plekken;
        List<long> confirmedTickets = new List<long>();
        
        Console.WriteLine(Localization.Start_Tour_Checkin);
        while (tour!.RegisteredTickets.Count > confirmedTickets.Count) {
            var ticketNumber = UserInput.GetNumber($"Ticket {confirmedTickets.Count + 1}/{tour.RegisteredTickets.Count}: ", 1);
            if (ticketNumber == null)
            {
                Console.WriteLine(Localization.Ongeldige_invoer);
                continue;
            }

            // If user wants to stop scanning tickets
            if (ticketNumber == 1)
            {
                break;
            }
            
            if (tour.RegisteredTickets.Contains(ticketNumber.Value))
            {
                confirmedTickets.Add(ticketNumber.Value);
                UpdateMenuState();
                continue;
            }
        
            // If there are less reservations than max openings, add user anyway
            if (maxSpots > tour.RegisteredTickets.Count)
            {
                Console.WriteLine(Localization.Ticket_niet_in_reserveringen);
                tour.RegisteredTickets.Add(ticketNumber.Value);
                depotContext.SaveChanges();
            }
            
            // 
            if (maxSpots <= tour.RegisteredTickets.Count)
            {
                Console.WriteLine(Localization.Rondleiding_Vol);
            }
        }

        // Remove absent tickets from the tour
        var presentTickets = tour.RegisteredTickets.Intersect(confirmedTickets).ToList();
        tour.RegisteredTickets = presentTickets;
        depotContext.SaveChanges();
        
        // If tickets are available to be added, we try to fill up the tour
        Console.WriteLine(Localization.Plekken_vrij_toevoegen);
        while (confirmedTickets.Count < maxSpots)
        {
            var ticketNumber = UserInput.GetNumber($"Ticket {tour.RegisteredTickets.Count + 1}/{maxSpots}: ", 1);

            if (ticketNumber == 1)
            {
                break;
            }
            
            tour.RegisteredTickets.Add(ticketNumber!.Value);
            depotContext.SaveChanges();
        }
        
        Console.WriteLine(Localization.Rondleiding_Gestart);
        tour.Departed = true;
        UpdateMenuState();
        ResetMenuState();
    }
    
    private static void RemoveVisitor(long tourId)
    {
        var ticketNummer = UserInput.GetNumber(Localization.Scan_uw_ticket, 1);
        if(ticketNummer == null)
        {
            ResetMenuState();
            return;
        }

        var tour = depotContext.Tours.Where(t => t.Id == tourId).FirstOrDefault();
        if (tour != null)
        {
            if (tour.RegisteredTickets.Contains(ticketNummer.Value))
            {
                tour.RegisteredTickets.Remove(ticketNummer.Value);
                depotContext.SaveChanges();
                Console.WriteLine(Localization.Ticket_verwijderd);
                ResetMenuState();
                return;
            }

            Console.WriteLine(Localization.Aanmelding_niet_gevonden);
            ResetMenuState();
        }
    }
    private static void AddVisitor(long tourId)
    {
        var ticketNummer = UserInput.GetNumber(Localization.Scan_uw_ticket, 1);
        if (ticketNummer == null)
        {
            ResetMenuState();
            return;
        }

        var tour = depotContext.Tours.Where(t => t.Id == tourId).FirstOrDefault();
        if (tour != null)
        {
            if (tour.RegisteredTickets.Contains(ticketNummer.Value))
            {
                Console.WriteLine(Localization.Ticket_al_geregistreerd);
                ResetMenuState();
                return;
            }

            tour.RegisteredTickets.Add(ticketNummer.Value);
            depotContext.SaveChanges();
            Console.WriteLine(Localization.Ticket_toegevoegd);
            ResetMenuState();
        }
    }

    private static void GetTour(long tourNr)
    {
        var tour = depotContext.Tours.Where(t => t.Id == tourNr).FirstOrDefault();

        Console.WriteLine($"{Localization.Rondleiding_om} {tour?.Start.ToString("HH:mm")}");
        Console.WriteLine(Localization.Alle_ticketnummers);
        foreach (var ticketNummer in tour!.RegisteredTickets)
        {
            Console.WriteLine($"{Localization.Ticket} {ticketNummer}");
        }

        ResetMenuState();
    }

    private static bool GetAccount(out User? user, List<Role> allowedRoles)
    {
        while (true)
        {
            var userId = UserInput.GetNumber(Localization.Scan_uw_pas, 1);

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
            consoleMenu.SetListItems(CreateTourList());
            Console.WriteLine(Localization.Ga_terug);
            consoleMenu.Reset();
            Console.ReadLine();
        }
    }

    public static void UpdateMenuState()
    {
        consoleMenu?.SetListItems(CreateTourList());
    }
}