using System.Data;

namespace SeatReservation.Infrastructure.Postgre.Database;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}