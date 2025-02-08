using HelpDesk.Management.Api.Controllers;
using HelpDesk.Management.Application.Incidents.Commands;
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
    public async Task GivenValidRequest_WhenLoggingIncident_ThenReturnsOkResult()
    {
        // Arrange
        var request = new LogIncidentRequest(
            "System Error",
            "Application crashes on startup",
            "john.doe@example.com"
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<LogIncidentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _controller.LogIncident(request);

        // Assert
        result.Should().BeOfType<OkResult>();
        _mediatorMock.Verify(
            x => x.Send(
                It.Is<LogIncidentCommand>(cmd =>
                    cmd.Title == request.Title &&
                    cmd.Description == request.Description &&
                    cmd.ReportedBy == request.ReportedBy),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}