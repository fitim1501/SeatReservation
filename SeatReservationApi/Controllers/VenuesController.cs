using Microsoft.AspNetCore.Mvc;
using SeatReservation.Application;
using SeatReservation.Application.Venues;
using SeatReservation.Contracts;
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
    
    [HttpPatch("name")]
    public async Task<EndpointResult<Guid>> UpdateVenueName(
        [FromServices] UpdateVenueNameHandler handler, 
        [FromBody] UpdateVenueNameRequest request,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request, cancellationToken);
    }
    
    [HttpPatch("/name/by-prefix")]
    public async Task<IActionResult> UpdateVenueNameByPrefix(
        [FromServices] UpdateVenueNameByPrefixHandler handler, 
        [FromBody] UpdateVenueNameByPrefixRequest request,
        CancellationToken cancellationToken)
    {
        await handler.Handle(request, cancellationToken);

        return Ok();
    }
}