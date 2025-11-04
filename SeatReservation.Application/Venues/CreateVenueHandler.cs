using CSharpFunctionalExtensions;
using SeatReservation.Application.DataBase;
using SeatReservation.Contracts;
using SeatReservation.Contracts.Venues;
using SeatReservation.Domain.Venues;
using Shared;

namespace SeatReservation.Application.Venues;

public class CreateVenueHandler
{
    private readonly IVenuesRepository _venuesRepository;
    
    public CreateVenueHandler(IVenuesRepository venuesRepository)
    {
        _venuesRepository = venuesRepository;
    }
    /// <summary>
    /// Этот метод создает площадку со всеми местами.
    /// </summary>
    public async Task<Result<Guid, Error>> Handle(CreateVenueRequest request, CancellationToken cancellationToken)
    {
        // бизнес валидация
        
        // создание доменных моделей

        var venue = Venue.Create(request.Prefix, request.Name, request.SeatsLimit);
        if (venue.IsFailure)
        {
            return venue.Error;
        }

        //сохранение доменных моделей в базу данных
        
        List<Seat> seats = [];
        foreach (var seatRequest in  request.Seats)
        {
            var seat = Seat.Create(venue.Value, seatRequest.RowNumber, seatRequest.SeatNumber);
            if (seat.IsFailure)
            {
                return seat.Error;
            }
            
            seats.Add(seat.Value);
        }

        venue.Value.AddSeats(seats);
        
        await _venuesRepository.Add(venue.Value, cancellationToken);
        
        return venue.Value.Id.Value;
    }
}