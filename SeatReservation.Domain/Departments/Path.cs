namespace SeatReservation.Domain.Departments;

public record Path
{
    private Path(string value)
    {
        Value = value;
    }
    public string Value { get; }
    
    public static Path Create(string name)
    {
        return new Path(name);
    }
}