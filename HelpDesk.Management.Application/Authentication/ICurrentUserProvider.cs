namespace HelpDesk.Management.Application.Authentication;

public interface ICurrentUserProvider
{
    string UserId { get; }
}
