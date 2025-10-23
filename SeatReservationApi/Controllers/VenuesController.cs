using Microsoft.AspNetCore.Mvc;
using SeatReservation.Application;
using SeatReservation.Domain.Venues;
using Shared;
using Shared.EndpointsResults;

namespace SeatReservationApi.Controllers;

[ApiController]
[Route("api/venues")]
public class VenuesController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Envelope<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope>(StatusCodes.Status302Found)]
    public async Task<EndpointResult<Guid>> CreateVenue(
        [FromServices] CreateVenueHandler handler, 
        [FromBody] CreateVenueRequest request,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request, cancellationToken);
    }
}