namespace SeatReservation.Domain.Departments;

public record DepartmentName
{
    private DepartmentName(string value)
    {
        Value = value;
    }
    public string Value { get; }
    
    public static DepartmentName Create(string name)
    {
        return new DepartmentName(name);
    }
}