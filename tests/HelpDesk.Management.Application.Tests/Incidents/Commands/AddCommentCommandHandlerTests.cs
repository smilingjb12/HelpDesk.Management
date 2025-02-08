using FluentAssertions;
using FluentResults;
using HelpDesk.Management.Application.Authentication;
using HelpDesk.Management.Application.Incidents.Commands;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using Moq;
using Xunit;

namespace HelpDesk.Management.Application.Tests.Incidents.Commands;

public class AddCommentCommandHandlerTests
{
  private readonly Mock<IIncidentRepository> _repositoryMock;
  private readonly Mock<ICurrentUserProvider> _currentUserProviderMock;
  private readonly AddCommentCommandHandler _handler;
  private readonly string _currentUserId = "user123";

  public AddCommentCommandHandlerTests()
  {
    _repositoryMock = new Mock<IIncidentRepository>();
    _currentUserProviderMock = new Mock<ICurrentUserProvider>();
    _currentUserProviderMock.Setup(x => x.UserId).Returns(_currentUserId);
    _handler = new AddCommentCommandHandler(_repositoryMock.Object, _currentUserProviderMock.Object);
  }

  [Fact]
  public async Task ValidCommand_HandlingAddComment_CommentIsAdded()
  {
    // Arrange
    var incidentId = Guid.NewGuid();
    var comment = "This is a test comment";
    var command = new AddCommentCommand(incidentId, comment);

    var incident = CreateTestIncident();
    _repositoryMock
        .Setup(x => x.GetById(incidentId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(incident));

    _currentUserProviderMock
        .Setup(x => x.UserId)
        .Returns(_currentUserId);

    _repositoryMock
        .Setup(x => x.Update(It.IsAny<Incident>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok());

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(incident.Id);
    _repositoryMock.Verify(x => x.Update(
        It.Is<Incident>(i => i.Id == incident.Id),
        It.IsAny<CancellationToken>()),
        Times.Once);
  }

  [Fact]
  public async Task IncidentNotFound_HandlingAddComment_ReturnsFailure()
  {
    // Arrange
    var incidentId = Guid.NewGuid();
    var comment = "This is a test comment";
    var command = new AddCommentCommand(incidentId, comment);

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
  public async Task UpdateFailure_HandlingAddComment_ReturnsFailure()
  {
    // Arrange
    var incidentId = Guid.NewGuid();
    var comment = "This is a test comment";
    var command = new AddCommentCommand(incidentId, comment);

    var incident = CreateTestIncident();
    _repositoryMock
        .Setup(x => x.GetById(incidentId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(incident));

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