using System.Security.Claims;

namespace Trpg.WebApi.Services
{
    public interface ITokenIssuer
    {
        string IssueToken(ClaimsIdentity claimsIdentity);
    }
}