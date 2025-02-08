using FluentResults;
using HelpDesk.Management.Application.Incidents.Commands;
using Marten;
using Moq;
using Xunit;

namespace HelpDesk.Management.Application.Tests.Incidents.Commands;

public class LogIncidentCommandHandlerTests
{
    private readonly Mock<IDocumentSession> _sessionMock;
    private readonly LogIncidentCommandHandler _handler;

    public LogIncidentCommandHandlerTests()
    {
        _sessionMock = new Mock<IDocumentSession>();
        _handler = new LogIncidentCommandHandler(_sessionMock.Object);
    }

    [Fact]
    public async Task GivenValidCommand_WhenHandlingLogIncident_ThenIncidentIsStored()
    {
        // Arrange
        var command = new LogIncidentCommand(
            "System Error",
            "Application crashes on startup",
            "john.doe@example.com"
        );

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _sessionMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}