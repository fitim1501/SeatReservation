using CSharpFunctionalExtensions;
using SeatReservation.Application.DataBase;
using SeatReservation.Domain.Venues;
using Shared;

namespace SeatReservation.Application;

public class CreateVenueHandler
{
    // private readonly IReservationServiceDbContext _dbContext;
    //
    // public CreateVenueHandler(IReservationServiceDbContext dbContext)
    // {
    //     _dbContext = dbContext;
    // }
    /// <summary>
    /// Этот метод создает площадку со всеми местами.
    /// </summary>
    public async Task<Result<Guid, Error>> Handle(CreateVenueRequest request, CancellationToken cancellationToken)
    {
        // бизнес валидация
        
        // создание доменных моделей
        List<Seat> seats = [];
        foreach (var seatRequest in  request.Seats)
        {
            var seat = Seat.Create(seatRequest.RowNumber, seatRequest.SeatNumber);
            if (seat.IsFailure)
            {
                return seat.Error;
            }
            
            seats.Add(seat.Value);
        }

        var venue = Venue.Create(request.Prefix, request.Name, request.SeatsLimit, seats);

        // сохранение доменных моделей в базу данных

        // await _dbContext.Venues.AddAsync(venue.Value, cancellationToken);
        //
        // await _dbContext.SaveChangesAsync(cancellationToken);

        return venue.Value.Id.Value;
    }
}