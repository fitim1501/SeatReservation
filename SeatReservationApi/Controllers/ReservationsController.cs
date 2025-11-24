using Microsoft.AspNetCore.Mvc;
using SeatReservation.Application.Reservations;
using SeatReservation.Application.Reservations.Commands;
using SeatReservation.Contracts;
using SeatReservation.Contracts.Reservations;
using Shared.EndpointsResults;

namespace SeatReservationApi.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Reserve(
        [FromBody]ReserveRequest request,
        [FromServices]ReserverHandler handler, 
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request, cancellationToken);
    }
    
    [HttpPost("/adjacent")]
    public async Task<EndpointResult<Guid>> ReserveAdjacentSeats(
        [FromBody] ReservedAdjacentSeatsRequest request,
        [FromServices] ReserveAdjacentSeatsHandler handler, 
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request, cancellationToken);
    }
}