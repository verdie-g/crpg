using System.Security.Claims;

namespace Crpg.WebApi.Services
{
    public interface ITokenIssuer
    {
        string IssueToken(ClaimsIdentity claimsIdentity);
    }
}