using Depot.Common.Navigation;
using System;
using System.Linq;

namespace Depot.Afdelingshoofd;

class Program
{
    private static Menu? consoleMenu;

    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        consoleMenu = new Menu("Afdelingshoofd", "Maak uw keuze uit het menu hieronder:");
        
        var afsluiten = new SubMenu('0', "Afsluiten", "Sluit het programma.", Close);
        consoleMenu.AddMenuItem(afsluiten);

        consoleMenu.Show();
    }

    private static void Close()
    {
        if (consoleMenu != null)
        {
            consoleMenu.IsShowing = false;
        }
    }
}