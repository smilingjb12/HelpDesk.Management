using FluentAssertions;
using HelpDesk.Management.Application.Incidents.Queries;
using HelpDesk.Management.Domain.Incidents;
using Moq;
using Xunit;

namespace HelpDesk.Management.Application.Tests.Incidents.Queries;

public class GetAllIncidentHistoryQueryHandlerTests
{
  private readonly Mock<IIncidentRepository> _repositoryMock;
  private readonly GetAllIncidentHistoryQueryHandler _handler;

  public GetAllIncidentHistoryQueryHandlerTests()
  {
    _repositoryMock = new Mock<IIncidentRepository>();
    _handler = new GetAllIncidentHistoryQueryHandler(_repositoryMock.Object);
  }

  [Fact]
  public async Task ValidQuery_HandlingGetAllIncidentHistory_ReturnsHistoryDtos()
  {
    // Arrange
    var incidentId = Guid.NewGuid();
    var query = new GetAllIncidentHistoryQuery(incidentId);
    var history = new List<IncidentHistoryEntry>
        {
            CreateTestHistoryEntry(incidentId, "StatusChanged", "Status changed to In Progress"),
            CreateTestHistoryEntry(incidentId, "CommentAdded", "New comment added")
        };

    _repositoryMock
        .Setup(x => x.GetIncidentHistory(incidentId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(history);

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Should().HaveCount(2);
    result.Should().AllSatisfy(dto =>
    {
      dto.Should().NotBeNull();
      dto.Id.Should().NotBeEmpty();
      dto.IncidentId.Should().Be(incidentId);
      dto.EventType.Should().NotBeNullOrEmpty();
      dto.Description.Should().NotBeNullOrEmpty();
      dto.PerformedBy.Should().NotBeNullOrEmpty();
      dto.Timestamp.Should().NotBe(default);
    });
  }

  [Fact]
  public async Task NoHistory_HandlingGetAllIncidentHistory_ReturnsEmptyList()
  {
    // Arrange
    var incidentId = Guid.NewGuid();
    var query = new GetAllIncidentHistoryQuery(incidentId);

    _repositoryMock
        .Setup(x => x.GetIncidentHistory(incidentId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<IncidentHistoryEntry>());

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Should().BeEmpty();
  }

  private static IncidentHistoryEntry CreateTestHistoryEntry(Guid incidentId, string eventType, string description)
  {
    var entry = new IncidentHistoryEntry();
    entry.Id = Guid.NewGuid();
    entry.IncidentId = incidentId;
    entry.EventType = eventType;
    entry.Description = description;
    entry.PerformedBy = "test@example.com";
    entry.Timestamp = DateTime.UtcNow;
    return entry;
  }
}