using Depot.Common;
using Depot.Common.Workflow;
using Depot.DAL;
using Depot.DAL.Models;

public class CreateToursFlow : Workflow
{
    public TimeSpan? BeginTime { get; private set; }
    public TimeSpan? EndTime { get; private set; }
    public DateTime? BeginDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public int? Interval { get; private set; }

    public CreateToursFlow(DepotContext context) : base(context)
    {
    }

    private bool SetProperty<T>(Action setAction, T value)
    {
        if (value == null)
        {
            return false;
        }

        setAction();
        return true;
    }

    public bool SetBeginTime(TimeSpan? beginTime) => SetProperty(() => { BeginTime = beginTime; }, beginTime);
    public bool SetEndTime(TimeSpan? endTime) => SetProperty(() => { EndTime = endTime; }, endTime);
    public bool SetBeginDate(DateTime? beginDate) => SetProperty(() => { BeginDate = beginDate; }, beginDate);
    public bool SetEndDate(DateTime? endDate) => SetProperty(() => { EndDate = endDate; }, endDate);
    public bool SetInterval(int? interval) => SetProperty(() => { Interval = interval; }, interval);

    public bool SetBeginAndEndDateByDays(int daysInTheFuture)
    {
        BeginDate = DateTime.Now.Date.AddDays(daysInTheFuture);
        EndDate = DateTime.Now.Date.AddDays(daysInTheFuture);
        return true;
    }

    public override string Validate(out bool valid)
    {
        if (BeginDate == null ||
            EndDate == null ||
            BeginTime == null ||
            EndTime == null ||
            Interval == null)
        {
            valid = false;
            return Localization.Niet_alle_gegevens_ingevuld;
        }

        var response = "";
        var dateCursor = BeginDate;
        while (dateCursor <= EndDate)
        {
            var startTime = dateCursor.Value.Date.AddMilliseconds(BeginTime.Value.TotalMilliseconds);
            var endTime = dateCursor.Value.Date.AddMilliseconds(EndTime.Value.TotalMilliseconds);

            List<Tour> tours = new List<Tour>();
            for (var time = startTime; time.AddMinutes(Globals.Tour_Length_in_minutes) < endTime; time = time.AddMinutes(Interval.Value))
            {
                tours.Add(new Tour { Start = time });
            }

            Context.Tours.AddRange(tours);

            foreach (var tour in tours)
            {
                response += $"{Localization.Rondleidingen_aangemaakt_voor} {tour.Start}.\n";
            }

            dateCursor = dateCursor.Value.AddDays(1);
        }
        Context.SaveChanges();
        valid = true;
        return response;
    }
}
