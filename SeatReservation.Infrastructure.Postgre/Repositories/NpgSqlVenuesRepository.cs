using CSharpFunctionalExtensions;
using Dapper;
using Microsoft.Extensions.Logging;
using SeatReservation.Application.DataBase;
using SeatReservation.Domain.Venues;
using SeatReservation.Infrastructure.Postgre.Database;
using Shared;

namespace SeatReservation.Infrastructure.Postgre.Repositories;

public class NpgSqlVenuesRepository : IVenuesRepository
{
    private readonly IDbConnectionFactory _conectionFactory;
    private readonly ILogger<NpgSqlVenuesRepository> _logger;

    public NpgSqlVenuesRepository(IDbConnectionFactory conectionFactory, ILogger<NpgSqlVenuesRepository> logger)
    {
        _conectionFactory = conectionFactory;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Error>> Add(Venue venue, CancellationToken cancellationToken)
    {
        using var connection = await _conectionFactory.CreateConnectionAsync(cancellationToken);

        using var transaction = connection.BeginTransaction();

        try
        {
            const string venueInsertSql = """
                                          INSERT INTO venues (id, prefix, name, "SeatsLimit")
                                          VALUES (@Id, @Prefix, @Name, @SeatsLimit)
                                          """;

            var venueInsertParams = new
            {
                Id = venue.Id.Value,
                Prefix = venue.Name.Prefix,
                Name = venue.Name.Name,
                SeatsLimit = venue.SeatsLimit
            };
        
            await connection.ExecuteAsync(venueInsertSql, venueInsertParams);

            if (!venue.Seats.Any())
            {
                return venue.Id.Value;
            }   
        
            const string seatsInsertSql = """
                                          INSERT INTO seats (id, row_number, seat_number, venue_id) 
                                          VALUES (@Id, @RowNumber, @SeatNumber, @VenueId)
                                          """;
        
            var seatsInsertParams = venue.Seats.Select(s => new
            {
                Id = s.Id.Value,
                RowNumber = s.RowNumber,
                SeatNumber = s.SeatNumber,
                VenueId = venue.Id.Value
            });
        
            await connection.ExecuteAsync(seatsInsertSql, seatsInsertParams);
            
            transaction.Commit();

            return venue.Id.Value;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            
            _logger.LogError(ex, "Fail to insert venue");
            
            return Error.Failure("venue.insert", "Fail to insert venue");
        }
    }

    public async Task<Result<Guid, Error>> UpdateVenueName(VenueId venueId, VenueName venueName, CancellationToken cancellationToken)
    {
        using var connection = await _conectionFactory.CreateConnectionAsync(cancellationToken);

        using var transaction = connection.BeginTransaction();

        try
        {
            const string updateNameSql = """
                                          UPDATE venues
                                          SET name = @Name
                                          WHERE id = @Id
                                          """;

            var updateNameParams = new
            {
                Id = venueId.Value,
                Name = venueName.Name
            };
        
            await connection.ExecuteAsync(updateNameSql, updateNameParams);

            transaction.Commit();

            return venueId.Value;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            
            _logger.LogError(ex, "Fail to update venue");
            
            return Error.Failure("venue.update", "Fail to update venue");
        }
    }

    public Task Save()
    {
        throw new NotImplementedException();
    }

    public async Task<UnitResult<Error>> UpdateVenueNameByPrefix(string prefix, VenueName venueName, CancellationToken cancellationToken)
    {
        using var connection = await _conectionFactory.CreateConnectionAsync(cancellationToken);

        using var transaction = connection.BeginTransaction();

        try
        {
            const string updateNameSql = """
                                         UPDATE venues
                                         SET name = @Name
                                         WHERE prefix LIKE @Prefix
                                         """;

            var updateNameParams = new
            {
                Prefix = $"{prefix}%",
                Name = venueName.Name
            };
        
            await connection.ExecuteAsync(updateNameSql, updateNameParams);

            transaction.Commit();

            return  UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            
            _logger.LogError(ex, "Fail to update venue");
            
            return Error.Failure("venue.update", "Fail to update venue");
        }
    }

    public async Task<Result<Venue, Error>> GeyById(VenueId venueId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}