using System.Security.Claims;

namespace Crpg.Web.Services
{
    public interface ITokenIssuer
    {
        string IssueToken(ClaimsIdentity claimsIdentity);
    }
}