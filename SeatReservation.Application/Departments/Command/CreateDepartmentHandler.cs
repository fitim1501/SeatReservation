using CSharpFunctionalExtensions;
using SeatReservation.Contracts.Venues;
using SeatReservation.Domain.DepartmentLocation;
using SeatReservation.Domain.Departments;
using Shared;

namespace SeatReservation.Application.Departments.Command;

public class CreateDepartmentHandler
{
    private readonly IDepartmentRepository _departmentRepository;

    public CreateDepartmentHandler(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }
    
    public async Task<Result<Guid, Error>> Handle(CancellationToken cancellationToken)
    {
        var rootId = new DepartmentId(Guid.NewGuid());
        var engId = new DepartmentId(Guid.NewGuid());
        var hrId = new DepartmentId(Guid.NewGuid());
        var salesId = new DepartmentId(Guid.NewGuid());
        var backendId = new DepartmentId(Guid.NewGuid());
        
        var root = Department.CreateParent(
            DepartmentName.Create("Head Office"),
            Identifier.Create("head-office"),
            rootId).Value;
        
        var eng = Department.CreateChild(
            DepartmentName.Create("Engineering"),
            Identifier.Create("engineering"),
            root,
            engId).Value;
        
        var backend = Department.CreateChild(
            DepartmentName.Create("Backend"),
            Identifier.Create("backend"),
            eng,
            backendId).Value;
        
        var hr = Department.CreateChild(
            DepartmentName.Create("Human Resources"),
            Identifier.Create("hr"),
            root,
            hrId).Value;
        
        var sales = Department.CreateChild(
            DepartmentName.Create("Sales"),
            Identifier.Create("sales"),
            root,
            salesId).Value;
        
        var result = await _departmentRepository.Create([root, eng, backend, hr, sales], cancellationToken);
        if (result.IsFailure)
        {
            return result.Error;
        }

        return new Result<Guid, Error>();
    }
}