using CSharpFunctionalExtensions;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeatReservation.Application.DataBase;
using SeatReservation.Application.Venues;
using SeatReservation.Domain.Venues;
using Shared;

namespace SeatReservation.Infrastructure.Postgre.Repositories;

public class VenuesRepository : IVenuesRepository
{
    private readonly ReservationServiceDbContext _dbContext;
    private readonly ILogger<VenuesRepository> _logger;

    public VenuesRepository(ReservationServiceDbContext dbContext, ILogger<VenuesRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Venue, Error>> GetById(VenueId venueId, CancellationToken cancellationToken)
    {
        var venue = await _dbContext.Venues
            .FirstOrDefaultAsync(v => v.Id == venueId, cancellationToken);
        
        if (venue is null)
        {
            return Error.NotFound("venue.not.found", "Venue not found");
        }

        var entries = _dbContext.ChangeTracker.Entries();
        
        return venue;
    }
    public async Task<Result<Venue, Error>> GetByIdWithSeats(VenueId venueId, CancellationToken cancellationToken)
    {
        var venue = await _dbContext.Venues
            .Include(v => v.Seats)
            .FirstOrDefaultAsync(v => v.Id == venueId, cancellationToken);
        
        if (venue is null)
        {
            return Error.NotFound("venue.not.found", "Venue not found");
        }

        var entries = _dbContext.ChangeTracker.Entries();
        
        return venue;
    }
    
    public async Task<IReadOnlyList<Venue>> GetByPrefix(string prefix, CancellationToken cancellationToken)
    {
        var venues = await _dbContext.Venues
            .Where(v => v.Name.Prefix.StartsWith(prefix))
            .ToListAsync(cancellationToken);

        var entries = _dbContext.ChangeTracker.Entries();
        
        return venues;
    }
    
    public async Task<Result<Guid, Error>> Add(Venue venue, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Venues.AddAsync(venue, cancellationToken);

            await _dbContext.SaveChangesAsync();
            
            return venue.Id.Value;
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fail to insert venue");
            
            return Error.Failure("venue.insert", "Fail to insert venue");
        }
    }

    public async Task Update(Venue venue)
    {
        var entries = _dbContext.ChangeTracker.Entries();

        _dbContext.Venues.Update(venue);
        
        var entries2 = _dbContext.ChangeTracker.Entries();
        
        await _dbContext.SaveChangesAsync();
    }
    public async Task<Result<Guid, Error>> UpdateVenueName(VenueId venueId, VenueName venueName, CancellationToken cancellationToken)
    {
        await _dbContext.Venues
            .Where(v => v.Id == venueId)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(v => v.Name.Name, venueName.Name), cancellationToken);

        return venueId.Value;
        
    }
    public async Task<UnitResult<Error>> UpdateVenueNameByPrefix(string prefix, VenueName venueName, CancellationToken cancellationToken)
    {
        await _dbContext.Venues
            .Where(v => v.Name.Prefix.StartsWith(prefix))
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(v => v.Name.Name, venueName.Name), cancellationToken);

        return UnitResult.Success<Error>();
    }
    
    public async Task<UnitResult<Error>> DeleteSeatsByVenuesId(VenueId venueId, CancellationToken cancellationToken)
    {
        await _dbContext.Seats
            .Where(s => s.Venue.Id == venueId)
            .ExecuteDeleteAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}