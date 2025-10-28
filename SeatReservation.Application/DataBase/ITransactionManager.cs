using CSharpFunctionalExtensions;
using Shared;

namespace SeatReservation.Application.DataBase;

public interface ITransactionManager
{
    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> SaveChangeAsync(CancellationToken cancellationToken = default);
}