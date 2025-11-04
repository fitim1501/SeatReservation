using Microsoft.AspNetCore.Mvc;
using SeatReservation.Application.Events;
using SeatReservation.Contracts.Events;

namespace SeatReservationApi.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    [HttpGet("/{eventId:guid}")]
    public async Task<ActionResult<GetEventDto>> GetById(
        [FromRoute] Guid eventId,
        [FromServices] GetEventByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var @event = await handler.Handle(new GeEventByIdRequest(eventId), cancellationToken);
        return Ok(@event);
    }
}