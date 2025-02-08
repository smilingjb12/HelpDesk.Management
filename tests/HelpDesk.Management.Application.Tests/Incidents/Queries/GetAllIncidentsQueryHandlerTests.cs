using FluentAssertions;
using FluentResults;
using HelpDesk.Management.Application.Incidents.Queries;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using Moq;
using Xunit;

namespace HelpDesk.Management.Application.Tests.Incidents.Queries;

public class GetAllIncidentsQueryHandlerTests
{
  private readonly Mock<IIncidentRepository> _repositoryMock;
  private readonly GetAllIncidentsQueryHandler _handler;

  public GetAllIncidentsQueryHandlerTests()
  {
    _repositoryMock = new Mock<IIncidentRepository>();
    _handler = new GetAllIncidentsQueryHandler(_repositoryMock.Object);
  }

  [Fact]
  public async Task ValidQuery_HandlingGetAllIncidents_ReturnsIncidentDtos()
  {
    // Arrange
    var query = new GetAllIncidentsQuery();
    var incidents = new List<Incident>
        {
            CreateTestIncident(),
            CreateTestIncident()
        };

    _repositoryMock
        .Setup(x => x.GetAll(It.IsAny<CancellationToken>()))
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
    });
  }

  [Fact]
  public async Task EmptyRepository_HandlingGetAllIncidents_ReturnsEmptyList()
  {
    // Arrange
    var query = new GetAllIncidentsQuery();
    _repositoryMock
        .Setup(x => x.GetAll(It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<Incident>());

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Should().BeEmpty();
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