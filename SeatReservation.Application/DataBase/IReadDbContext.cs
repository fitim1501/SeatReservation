using SeatReservation.Domain.Events;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Application.DataBase;

public interface IReadDbContext
{
    public IQueryable<Event> EventsRead { get; }
    public IQueryable<Venue> VenuesRead { get; }
    public IQueryable<Seat> SeatsRead { get; }
    public IQueryable<Reservation> ReservationsRead { get; }
    public IQueryable<ReservationSeat> ReservationSeatsRead { get; }
}