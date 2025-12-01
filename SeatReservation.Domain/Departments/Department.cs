using System.Reflection.Metadata.Ecma335;
using CSharpFunctionalExtensions;
using Shared;

namespace SeatReservation.Domain.Departments;

public class Department
{
    private Department(
        DepartmentId id, 
        DepartmentId? parentId,
        DepartmentName name, 
        Identifier identifier,
        Path path, 
        int depth)
    {
        Id = id;
        Name = name;
        Identifier = identifier;
        ParentId = parentId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Path = path;
        Depth = depth;
    }
    
    public static Result<Department, Error> CreateParent(
        DepartmentName name, 
        Identifier identifier,
        DepartmentId? departmentId = null)
    {
        var path = Path.CreateParent(identifier);
        
        var department = new Department(
            departmentId ?? new DepartmentId(Guid.NewGuid()),
            null,
            name,
            identifier,
            path,
            0);
        
        return department;
    }
    
    public static Result<Department, Error> CreateChild(
        DepartmentName name,
        Identifier identifier,
        Department parent,
        DepartmentId? departmentId = null)
    {
        var path = parent.Path.CreateChild(identifier);
        
        var department = new Department(
            departmentId ?? new DepartmentId(Guid.NewGuid()),
            parent.Id,
            name,
            identifier,
            path,
            parent.Depth + 1);

        return department;
    }
    
    // private readonly List<DepartmentLocation> _locations = [];
    // private readonly List<DepartmentPosition> _positions = [];

    public DepartmentId Id { get; private set; }
    
    public DepartmentName Name { get; private set; }
    
    public Identifier Identifier { get; private set; }
    
    public DepartmentId? ParentId { get; private set; }
    
    public Department? Parent { get; private set; }
    
    public Path Path { get; private set; }
    
    public int Depth { get; private set; }

    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set;  }
}