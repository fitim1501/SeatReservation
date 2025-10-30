using System.Data;
using CSharpFunctionalExtensions;
using Shared;

namespace SeatReservation.Application.DataBase;

public interface ITransactionManager
{
    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken = default, IsolationLevel? level = null);

    Task<UnitResult<Error>> SaveChangeAsync(CancellationToken cancellationToken = default);
}