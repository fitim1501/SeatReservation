using System.Data;

namespace SeatReservation.Application.DataBase;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}