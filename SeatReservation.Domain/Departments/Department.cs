namespace SeatReservation.Domain.Departments;

public class Department
{
    private Department()
    {
        
    }
    
    // private readonly List<DepartmentLocation> _locations = [];
    // private readonly List<DepartmentPosition> _positions = [];

    public DepartmentId Id { get; private set; } = null!;
    
    public DepartmentName Name { get; private set; } = null!;
    
    public Identifier Identifier { get; private set; } = null!;
    
    public DepartmentId? ParentId { get; private set; }
    
    public Department? Parent { get; private set; }
    public Path Path { get; private set; } = null!;
    
    public int Depth { get; private set; }

    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set;  }
}