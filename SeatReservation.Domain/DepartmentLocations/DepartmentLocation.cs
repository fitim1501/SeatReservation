using SeatReservation.Domain.Departments;

namespace SeatReservation.Domain.DepartmentLocation;

public class DepartmentLocation
{
    public DepartmentLocationId Id { get; }
    
    public DepartmentId DepartmentId { get; }
    private DepartmentLocation(DepartmentLocationId id, DepartmentId departmentId)
    {
        Id = id;
        DepartmentId = departmentId;
    }
}