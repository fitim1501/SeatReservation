using Microsoft.AspNetCore.Mvc;
using SeatReservation.Application;
using SeatReservation.Domain.Venues;

namespace SeatReservationApi.Controllers;

[ApiController]
[Route("api/venues")]
public class VenuesController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> CreateVenue(
        [FromServices] CreateVenueHandler handler, 
        [FromBody] CreateVenueRequest request,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request, cancellationToken);
        
        return result.Value;
    }
}