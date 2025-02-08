using FluentAssertions;
using HelpDesk.Management.Domain.Incidents;
using Xunit;

namespace HelpDesk.Management.Domain.Tests.Incidents;

public class IncidentTests
{
    [Fact]
    public void GivenValidDetails_WhenCreatingIncident_ThenIncidentIsCreated()
    {
        // Arrange
        var title = "System Error";
        var description = "Application crashes on startup";
        var reportedBy = "john.doe@example.com";

        // Act
        var incident = Incident.Create(title, description, reportedBy);

        // Assert
        incident.Should().NotBeNull();
        incident.Title.Should().Be(title);
        incident.Description.Should().Be(description);
        incident.ReportedBy.Should().Be(reportedBy);
        incident.Status.Should().Be(IncidentStatus.Open);
        incident.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void GivenInvalidTitle_WhenCreatingIncident_ThenThrowsArgumentException(string invalidTitle)
    {
        // Arrange
        var description = "Application crashes on startup";
        var reportedBy = "john.doe@example.com";

        // Act
        var act = () => Incident.Create(invalidTitle, description, reportedBy);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Title cannot be empty or whitespace.");
    }
}