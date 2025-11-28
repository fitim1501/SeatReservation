namespace SeatReservation.Domain.Departments;

public record Identifier
{
    private Identifier(string value)
    {
        Value = value;
    }
    public string Value { get; }
    
    public static Identifier Create(string name)
    {
        return new Identifier(name);
    }
}