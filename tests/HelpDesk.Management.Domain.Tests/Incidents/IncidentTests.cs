using FluentAssertions;
using FluentResults;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using HelpDesk.Management.Domain.Incidents.Validation;
using Moq;
using Xunit;

namespace HelpDesk.Management.Domain.Tests.Incidents;

public class IncidentTests
{
    private readonly Mock<IIncidentValidator> _validatorMock;

    public IncidentTests()
    {
        _validatorMock = new Mock<IIncidentValidator>();
        _validatorMock.Setup(x => x.CheckNotAssignedToIncidentAlready(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
    }

    [Fact]
    public void ValidDetails_CreatingIncident_IncidentIsCreated()
    {
        // Arrange
        var title = "System Error";
        var description = "Application crashes on startup";
        var reportedBy = "john.doe@example.com";
        var priority = Priority.High;
        var reportedAt = DateTime.UtcNow;

        var evt = new IncidentLogged(
            Title: title,
            Description: description,
            ReportedBy: reportedBy,
            ReportedAt: reportedAt,
            Priority: priority
        );

        // Act
        var incident = Incident.Create(evt);

        // Assert
        incident.Should().NotBeNull();
        incident.Title.Should().Be(title);
        incident.Description.Should().Be(description);
        incident.ReportedBy.Should().Be(reportedBy);
        incident.Status.Should().Be(IncidentStatus.New);
        incident.ReportedAt.Should().Be(reportedAt);
        incident.Priority.Should().Be(priority);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void InvalidTitle_CreatingIncident_ThrowsArgumentException(string invalidTitle)
    {
        // Arrange
        var description = "Application crashes on startup";
        var reportedBy = "john.doe@example.com";
        var priority = Priority.High;
        var reportedAt = DateTime.UtcNow;

        var evt = new IncidentLogged(
            Title: invalidTitle,
            Description: description,
            ReportedBy: reportedBy,
            ReportedAt: reportedAt,
            Priority: priority
        );

        // Act
        var act = () => Incident.Create(evt);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Title cannot be empty or whitespace.");
    }

    [Fact]
    public async Task InvalidAssignment_AssigningIncident_ReturnsFailure()
    {
        // Arrange
        var incident = CreateTestIncident();
        var assignedTo = "invalid-email";
        var expectedError = "Invalid email format";
        _validatorMock
            .Setup(x => x.CheckNotAssignedToIncidentAlready(assignedTo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(expectedError));

        // Act
        var result = await incident.Assign(assignedTo, _validatorMock.Object);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedError);
        incident.AssignedTo.Should().BeEmpty();
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

    private static IncidentStatus GetNextValidStatus(IncidentStatus currentStatus)
    {
        return currentStatus switch
        {
            IncidentStatus.New => IncidentStatus.InProgress,
            IncidentStatus.InProgress => IncidentStatus.Resolved,
            IncidentStatus.Resolved => IncidentStatus.Closed,
            _ => throw new ArgumentException($"Unexpected status: {currentStatus}")
        };
    }
}