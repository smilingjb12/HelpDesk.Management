using Asp.Versioning;
using HelpDesk.Management.Application.Incidents.Commands;
using HelpDesk.Management.Application.Incidents.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace HelpDesk.Management.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion(1)]
// todo: add auhentication
public class IncidentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public IncidentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Logs a new incident", Description = "Creates and logs a new incident in the system.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Incident successfully logged.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input data.")]
    public async Task<IActionResult> LogIncident(LogIncidentCommand command)
    {
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetIncident), new { id = result.Value }, result.Value)
            : BadRequest(result.Errors);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetIncident(Guid id)
    {
        var result = await _mediator.Send(new GetIncidentQuery(id));

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound();
    }

    [HttpPost("{id}/assign")]
    public async Task<IActionResult> AssignIncident(Guid id, AssignIncidentCommand command)
    {
        command = command with { IncidentId = id };
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Errors);
    }

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(Guid id, AddCommentCommand command)
    {
        command = command with { IncidentId = id };
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Errors);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllIncidents()
    {
        var result = await _mediator.Send(new GetAllIncidentHistory());

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Errors);
    }
}