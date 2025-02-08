using HelpDesk.Management.Application.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HelpDesk.Management.Infrastructure.Authentication;

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            return userId;
        }
    }
}
