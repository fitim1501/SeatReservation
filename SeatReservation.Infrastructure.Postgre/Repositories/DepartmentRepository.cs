using CSharpFunctionalExtensions;
using SeatReservation.Application.Departments;
using SeatReservation.Domain.Departments;
using Shared;

namespace SeatReservation.Infrastructure.Postgre.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ReservationServiceDbContext _dbContext;

    public DepartmentRepository(ReservationServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UnitResult<Error>> Create(List<Department> departments,CancellationToken cancellationToken)
    {
        await _dbContext.Departments.AddRangeAsync(departments, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return UnitResult.Success<Error>();
    }
}