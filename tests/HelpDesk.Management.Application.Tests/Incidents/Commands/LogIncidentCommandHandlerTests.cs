using FluentAssertions;
using FluentResults;
using HelpDesk.Management.Application.Incidents.Commands;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using Moq;
using Xunit;

namespace HelpDesk.Management.Application.Tests.Incidents.Commands;

public class LogIncidentCommandHandlerTests
{
    private readonly Mock<IIncidentRepository> _repositoryMock;
    private readonly LogIncidentCommandHandler _handler;

    public LogIncidentCommandHandlerTests()
    {
        _repositoryMock = new Mock<IIncidentRepository>();
        _handler = new LogIncidentCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task ValidCommand_HandlingLogIncident_IncidentIsStored()
    {
        // Arrange
        var command = new LogIncidentCommand(
            "System Error",
            "Application crashes on startup",
            "john.doe@example.com",
            Priority.High
        );

        var expectedId = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.Create(It.IsAny<Incident>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(expectedId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedId);
        _repositoryMock.Verify(x => x.Create(
            It.Is<Incident>(i =>
                i.Title == command.Title &&
                i.Description == command.Description &&
                i.ReportedBy == command.ReportedBy &&
                i.Priority == command.Priority),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RepositoryFailure_HandlingLogIncident_ReturnsFailure()
    {
        // Arrange
        var command = new LogIncidentCommand(
            "System Error",
            "Application crashes on startup",
            "john.doe@example.com",
            Priority.High
        );

        var expectedError = "Failed to create incident";
        _repositoryMock
            .Setup(x => x.Create(It.IsAny<Incident>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<Guid>(expectedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedError);
    }
}