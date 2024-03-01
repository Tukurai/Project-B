namespace Depot.Common.Models;

public class TourModel
{
    public int TourId;
    public DateTime TourDateTime;
    public List<string>? ReservationIDs;
    public List<string>? WaitlistIDs;
}