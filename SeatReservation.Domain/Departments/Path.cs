namespace SeatReservation.Domain.Departments;

public record Path
{
    private const char Separator = '.';
    public string Value { get; }
    
    private Path(string value)
    {
        Value = value;
    }
    
    public static Path Create(string name)
    {
        return new Path(name);
    }
    
    public static Path CreateParent(Identifier identifier)
    {
        return new Path(identifier.Value);
    }
    
    public Path CreateChild(Identifier identifier)
    {
        return new Path(Value + Separator + identifier.Value);
    }
}