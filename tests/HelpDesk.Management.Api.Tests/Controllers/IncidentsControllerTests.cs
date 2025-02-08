using FluentAssertions;
using FluentResults;
using HelpDesk.Management.Api.Controllers;
using HelpDesk.Management.Application.Incidents.Commands;
using HelpDesk.Management.Application.Incidents.Queries;
using HelpDesk.Management.Domain.Incidents;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HelpDesk.Management.Api.Tests.Controllers;

public class IncidentsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly IncidentsController _controller;

    public IncidentsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new IncidentsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task ValidRequest_LoggingIncident_ReturnsOkResult()
    {
        // Arrange
        var request = new LogIncidentCommand(
            "System Error",
            "Application crashes on startup",
            "john.doe@example.com",
            Priority.High
        );

        var expectedId = Guid.NewGuid();
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<LogIncidentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(expectedId));

        // Act
        var result = await _controller.LogIncident(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().Be(expectedId);
        createdResult.ActionName.Should().Be(nameof(IncidentsController.GetIncident));
        _mediatorMock.Verify(
            x => x.Send(
                It.Is<LogIncidentCommand>(cmd =>
                    cmd.Title == request.Title &&
                    cmd.Description == request.Description &&
                    cmd.ReportedBy == request.ReportedBy &&
                    cmd.Priority == request.Priority),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidId_GettingIncident_ReturnsOkResult()
    {
        // Arrange
        var incidentId = Guid.NewGuid();
        var incident = new IncidentDto(
            Id: incidentId,
            Title: "System Error",
            Description: "Application crashes on startup",
            ReportedBy: "john.doe@example.com",
            ReportedAt: DateTime.UtcNow,
            Status: IncidentStatus.New.ToString(),
            Priority: Priority.High.ToString(),
            AssignedTo: null,
            Comments: []
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetIncidentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(incident));

        // Act
        var result = await _controller.GetIncident(incidentId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(incident);
        _mediatorMock.Verify(
            x => x.Send(
                It.Is<GetIncidentQuery>(q => q.Id == incidentId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvalidId_GettingIncident_ReturnsNotFound()
    {
        // Arrange
        var incidentId = Guid.NewGuid();
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetIncidentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IncidentDto>("Incident not found"));

        // Act
        var result = await _controller.GetIncident(incidentId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ValidRequest_ChangingStatus_ReturnsOkResult()
    {
        // Arrange
        var incidentId = Guid.NewGuid();
        var newStatus = IncidentStatus.InProgress;
        var command = new ChangeStatusCommandCommand(incidentId, newStatus);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ChangeStatusCommandCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(incidentId));

        // Act
        var result = await _controller.ChangeStatus(incidentId, command);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mediatorMock.Verify(
            x => x.Send(
                It.Is<ChangeStatusCommandCommand>(cmd =>
                    cmd.IncidentId == incidentId &&
                    cmd.NewStatus == newStatus),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidRequest_GettingAssignedIncidents_ReturnsOkResult()
    {
        // Arrange
        var userId = "jane.doe@example.com";
        var incidents = new List<IncidentSummaryDto>
        {
            new(
                Id: Guid.NewGuid(),
                Title: "System Error",
                Status: IncidentStatus.New.ToString(),
                Priority: Priority.High.ToString(),
                ReportedBy: "john.doe@example.com",
                ReportedAt: DateTime.UtcNow,
                AssignedTo: userId
            )
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetAllIncidentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(incidents);

        // Act
        var result = await _controller.GetAllIncidents();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(incidents);
    }

    [Fact]
    public async Task ValidRequest_GettingIncidentHistory_ReturnsOkResult()
    {
        // Arrange
        var incidentId = Guid.NewGuid();
        var history = new List<IncidentHistoryDto>
        {
            new(
                Id: Guid.NewGuid(),
                IncidentId: incidentId,
                EventType: "StatusChanged",
                Description: "Status changed from New to InProgress",
                PerformedBy: "jane.doe@example.com",
                Timestamp: DateTime.UtcNow
            )
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetAllIncidentHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.GetAllIncidentHistory(incidentId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(history);
    }
}