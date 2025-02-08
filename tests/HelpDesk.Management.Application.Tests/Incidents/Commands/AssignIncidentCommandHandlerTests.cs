using FluentAssertions;
using FluentResults;
using HelpDesk.Management.Application.Authentication;
using HelpDesk.Management.Application.Incidents.Commands;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using HelpDesk.Management.Domain.Incidents.Validation;
using Moq;
using Xunit;

namespace HelpDesk.Management.Application.Tests.Incidents.Commands;

public class AssignIncidentCommandHandlerTests
{
    private readonly Mock<IIncidentRepository> _repositoryMock;
    private readonly Mock<ICurrentUserProvider> _currentUserProviderMock;
    private readonly Mock<IIncidentValidator> _validatorMock;
    private readonly AssignIncidentCommandHandler _handler;

    public AssignIncidentCommandHandlerTests()
    {
        _repositoryMock = new Mock<IIncidentRepository>();
        _currentUserProviderMock = new Mock<ICurrentUserProvider>();
        _validatorMock = new Mock<IIncidentValidator>();
        _handler = new AssignIncidentCommandHandler(
            _repositoryMock.Object,
            _currentUserProviderMock.Object,
            _validatorMock.Object);
    }

    [Fact]
    public async Task IncidentNotFound_HandlingAssignIncident_ReturnsFailure()
    {
        // Arrange
        var incidentId = Guid.NewGuid();
        var assignedTo = "jane.doe@example.com";
        var command = new AssignIncidentCommand(incidentId, assignedTo);

        var expectedError = "Incident not found";
        _repositoryMock
            .Setup(x => x.GetById(incidentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<Incident>(expectedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedError);
    }

    [Fact]
    public async Task ValidationFailure_HandlingAssignIncident_ReturnsFailure()
    {
        // Arrange
        var incidentId = Guid.NewGuid();
        var assignedTo = "invalid-email";
        var command = new AssignIncidentCommand(incidentId, assignedTo);

        var incident = CreateTestIncident();
        _repositoryMock
            .Setup(x => x.GetById(incidentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(incident));

        var expectedError = "Invalid email format";
        _validatorMock
            .Setup(x => x.CheckNotAssignedToIncidentAlready(assignedTo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(expectedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedError);
    }

    [Fact]
    public async Task UpdateFailure_HandlingAssignIncident_ReturnsFailure()
    {
        // Arrange
        var incidentId = Guid.NewGuid();
        var assignedTo = "jane.doe@example.com";
        var command = new AssignIncidentCommand(incidentId, assignedTo);

        var incident = CreateTestIncident();
        _repositoryMock
            .Setup(x => x.GetById(incidentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(incident));

        _validatorMock
            .Setup(x => x.CheckNotAssignedToIncidentAlready(assignedTo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var expectedError = "Failed to update incident";
        _repositoryMock
            .Setup(x => x.Update(It.IsAny<Incident>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(expectedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedError);
    }

    private static Incident CreateTestIncident()
    {
        var evt = new IncidentLogged(
            Title: "Test Incident",
            Description: "Test Description",
            ReportedBy: "test@example.com",
            ReportedAt: DateTime.UtcNow,
            Priority: Priority.Medium
        );
        return Incident.Create(evt);
    }
}