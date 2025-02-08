using FluentAssertions;
using FluentResults;
using HelpDesk.Management.Application.Incidents.Queries;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using Moq;
using Xunit;

namespace HelpDesk.Management.Application.Tests.Incidents.Queries;

public class GetIncidentQueryHandlerTests
{
  private readonly Mock<IIncidentRepository> _repositoryMock;
  private readonly GetIncidentQueryHandler _handler;

  public GetIncidentQueryHandlerTests()
  {
    _repositoryMock = new Mock<IIncidentRepository>();
    _handler = new GetIncidentQueryHandler(_repositoryMock.Object);
  }

  [Fact]
  public async Task ValidQuery_HandlingGetIncident_ReturnsIncidentDto()
  {
    // Arrange
    var incidentId = Guid.NewGuid();
    var query = new GetIncidentQuery(incidentId);

    var incident = CreateTestIncident();
    _repositoryMock
        .Setup(x => x.GetById(incidentId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(incident));

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Id.Should().Be(incident.Id);
    result.Value.Title.Should().Be(incident.Title);
    result.Value.Description.Should().Be(incident.Description);
    result.Value.ReportedBy.Should().Be(incident.ReportedBy);
    result.Value.ReportedAt.Should().Be(incident.ReportedAt);
    result.Value.Priority.Should().Be(incident.Priority.ToString());
    result.Value.Status.Should().Be(incident.Status.ToString());
    result.Value.AssignedTo.Should().Be(incident.AssignedTo);
  }

  [Fact]
  public async Task IncidentNotFound_HandlingGetIncident_ReturnsFailure()
  {
    // Arrange
    var incidentId = Guid.NewGuid();
    var query = new GetIncidentQuery(incidentId);

    var expectedError = "Incident not found";
    _repositoryMock
        .Setup(x => x.GetById(incidentId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Fail<Incident>(expectedError));

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

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