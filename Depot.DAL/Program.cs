using Depot.DAL.Models;

namespace Depot.DAL;

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("This is an example of the JsonDataProvider");
        
        // First create a new instance of the JsonDataProvider,
        // specifying the type of object you want to work with
        IDataProvider<Tour> dataProvider = new JsonDataProvider<Tour>("tours");
        
        // There should already be a tour in the data storage
        var existingTour = dataProvider.Read(1);
        Console.WriteLine($"We retrieved a tour with id {existingTour.Id}");
        Console.WriteLine($"The tour starts at {existingTour.Start}");
        
        Console.WriteLine("-----------------");
        
        // Then create an instance of the model you want to add to the data storage
        var tour = new Tour
        {
            Id = 2,
            Start = DateTime.Now,
            Registrations = new List<int> {1, 2, 3},
            Queue = new List<int> {4, 5, 6}
        };
        
        //--------------------------------
        // CREATE
        dataProvider.Create(tour);
        Console.WriteLine($"We created a tour with id {tour.Id}");
        Console.WriteLine($"The tour starts at {tour.Start}");
        
        Console.WriteLine("-----------------");
        
        //--------------------------------
        // READ
        var retrievedTour = dataProvider.Read(2);
        Console.WriteLine($"We retrieved a tour with id {retrievedTour.Id}");
        Console.WriteLine($"The tour starts at {retrievedTour.Start}");
        
        
        //--------------------------------
        // UPDATE
        if (retrievedTour != null)
        {
            // One day from now
            retrievedTour.Start = DateTime.Now.AddDays(1);   
            // And update the storage
            dataProvider.Update(retrievedTour);
        }
        else
        {
            Console.WriteLine("Tour not found");
        }
        
        // Check if updates pushed through
        Console.WriteLine($"Current: {retrievedTour.Start}");
        var updatedTour = dataProvider.Read(2);
        Console.WriteLine($"Retrieved: {updatedTour.Start}");
        
        Console.WriteLine("-----------------");
        
        
        //--------------------------------
        // DELETE
        if (retrievedTour != null)
        {
            dataProvider.Delete(retrievedTour);
        }
        else
        {
            Console.WriteLine("Tour not found");
        }
    }
}