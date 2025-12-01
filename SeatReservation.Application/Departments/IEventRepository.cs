using CSharpFunctionalExtensions;
using SeatReservation.Domain.Departments;
using SeatReservation.Domain.Events;
using Shared;

namespace SeatReservation.Application.Departments;

public interface IDepartmentRepository
{
    Task<UnitResult<Error>> Create(List<Department> departments, CancellationToken cancellationToken);
}