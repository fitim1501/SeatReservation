using CSharpFunctionalExtensions;
using Shared;

namespace SeatReservation.Application.DataBase;

public interface ITransactionScope : IDisposable
{
    UnitResult<Error> Commit();

    UnitResult<Error> Rollback();
}