using FluentAssertions;
using HelpDesk.Management.Infrastructure.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using Xunit;

namespace HelpDesk.Management.Infrastructure.Tests.Authentication;

public class AuthenticationServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly CurrentUserProvider _service;

    public AuthenticationServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _service = new CurrentUserProvider(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void AuthenticatedUser_GettingCurrentUser_ReturnsUserId()
    {
        // Arrange
        var expectedUserId = "user123";
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, expectedUserId)
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(httpContext);

        // Act
        var result = _service.UserId;

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public void UnauthenticatedUser_GettingCurrentUser_ReturnsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(httpContext);

        // Act
        var result = _service.UserId;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NoHttpContext_GettingCurrentUser_ReturnsNull()
    {
        // Arrange
        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns((HttpContext)null);

        // Act
        var result = _service.UserId;

        // Assert
        result.Should().BeNull();
    }
}