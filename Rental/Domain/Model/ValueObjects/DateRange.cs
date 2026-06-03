namespace Moveo_backend.Rental.Domain.Model.ValueObjects;

public record DateRange
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    
    // Constructor vacío para EF Core
    protected DateRange() { }
    public DateRange(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.");

        StartDate = startDate;
        EndDate = endDate;
    }
}