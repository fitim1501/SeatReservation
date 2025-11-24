using Microsoft.AspNetCore.Mvc;
using SeatReservation.Application.Events;
using SeatReservation.Application.Events.Queries;
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
    
    [HttpGet("/{eventId:guid}/dapper")]
    public async Task<ActionResult<GetEventDto>> GetByIdDapper(
        [FromRoute] Guid eventId,
        [FromServices] GetEventByIdHandlerDapper handler,
        CancellationToken cancellationToken)
    {
        var @event = await handler.Handle(new GeEventByIdRequest(eventId), cancellationToken);
        return Ok(@event);
    }
}