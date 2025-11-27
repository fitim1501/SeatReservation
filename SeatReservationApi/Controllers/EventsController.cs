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
    
    [HttpGet]
    public async Task<ActionResult<GetEventDto>> Get(
        [FromQuery] GetEventsRequest request,
        [FromServices] GetEventsHandler handler,
        CancellationToken cancellationToken)
    {
        var @events = await handler.Handle(request, cancellationToken);
        return Ok(@events);
    }
    
    [HttpGet("dapper")]
    public async Task<ActionResult<GetEventDto>> GetDapper(
        [FromQuery] GetEventsRequest request,
        [FromServices] GetEventsHandlerDapper handler,
        CancellationToken cancellationToken)
    {
        var @events = await handler.Handle(request, cancellationToken);
        return Ok(@events);
    }
}