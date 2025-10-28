using Microsoft.AspNetCore.Mvc;
using SeatReservation.Application.Reservations;
using SeatReservation.Contracts;
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
}