using Microsoft.AspNetCore.Mvc;
using SeatReservation.Application.Departments.Command;
using SeatReservation.Application.Departments.Queries;
using SeatReservation.Application.Venues.Commands;
using SeatReservation.Contracts.Venues;
using Shared.EndpointsResults;

namespace SeatReservationApi.Controllers;


[ApiController]
[Route("api/departments")]
public class DepartmentsController : ControllerBase
{
    [HttpPost]
    
    public async Task<EndpointResult<Guid>> CreateVenue(
        [FromServices] CreateDepartmentHandler handler, 
        CancellationToken cancellationToken)
    {
        return await handler.Handle(cancellationToken);
    }
    
    [HttpGet()]
    public async Task<ActionResult> GetById(
        [FromServices] GetDepartmentsHandler handler,
        CancellationToken cancellationToken)
    {
        return Ok(await handler.Handle(cancellationToken));
    }
    
    [HttpGet("ltree")]
    public async Task<ActionResult> GetByLtree(
        [FromServices] GetDepartmentLtreeHandler handler,
        CancellationToken cancellationToken)
    {
        return Ok(await handler.Handle(cancellationToken));
    }
}