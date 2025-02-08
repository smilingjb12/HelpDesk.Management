using FluentAssertions;
using FluentResults;
using HelpDesk.Management.Application.Incidents.Queries;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using HelpDesk.Management.Domain.Incidents.Validation;
using Moq;
using Xunit;

namespace HelpDesk.Management.Application.Tests.Incidents.Queries;

public class GetAssignedIncidentsQueryHandlerTests
{
    private readonly Mock<IIncidentRepository> _repositoryMock;
    private readonly Mock<IIncidentValidator> _validatorMock;
    private readonly GetAssignedIncidentsQueryHandler _handler;

    public GetAssignedIncidentsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IIncidentRepository>();
        _validatorMock = new Mock<IIncidentValidator>();
        _validatorMock.Setup(x => x.CheckNotAssignedToIncidentAlready(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        _handler = new GetAssignedIncidentsQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task ValidQuery_HandlingGetAssignedIncidents_ReturnsIncidentDtos()
    {
        // Arrange
        var userId = "user123";
        var query = new GetAssignedIncidentsQuery(userId);
        var incidents = new List<Incident>
        {
            CreateTestIncident(assignedTo: userId),
            CreateTestIncident(assignedTo: userId)
        };

        _repositoryMock
            .Setup(x => x.GetAssignedIncidents(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(incidents);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(dto =>
        {
            dto.Should().NotBeNull();
            dto.Id.Should().NotBeEmpty();
            dto.Title.Should().NotBeNullOrEmpty();
            dto.Status.Should().NotBeNullOrEmpty();
            dto.Priority.Should().NotBeNullOrEmpty();
            dto.ReportedBy.Should().NotBeNullOrEmpty();
            dto.ReportedAt.Should().NotBe(default);
            dto.AssignedTo.Should().Be(userId);
        });
    }

    [Fact]
    public async Task NoAssignedIncidents_HandlingGetAssignedIncidents_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user123";
        var query = new GetAssignedIncidentsQuery(userId);

        _repositoryMock
            .Setup(x => x.GetAssignedIncidents(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    private Incident CreateTestIncident(string assignedTo = "")
    {
        var incident = Incident.Create(new IncidentLogged(
            "Test Title",
            "Test Description",
            "user1",
            DateTime.UtcNow,
            Priority.Medium));

        if (!string.IsNullOrEmpty(assignedTo))
        {
            incident.Apply(new IncidentAssigned(incident.Id, assignedTo, DateTime.UtcNow));
        }

        return incident;
    }
}