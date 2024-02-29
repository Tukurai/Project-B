using Depot.Common.Navigation;
using System;
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
        Console.WriteLine("Start reservation");
        Console.ReadLine();
    }
}