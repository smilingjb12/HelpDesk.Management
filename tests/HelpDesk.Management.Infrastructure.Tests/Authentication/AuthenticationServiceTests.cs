using HelpDesk.Management.Infrastructure.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace HelpDesk.Management.Infrastructure.Tests.Authentication;

public class AuthenticationServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _service = new AuthenticationService(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void GivenAuthenticatedUser_WhenGettingCurrentUser_ThenReturnsUserEmail()
    {
        // Arrange
        var expectedEmail = "john.doe@example.com";
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-User-Email"] = expectedEmail;

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(httpContext);

        // Act
        var result = _service.GetCurrentUserEmail();

        // Assert
        result.Should().Be(expectedEmail);
    }
}