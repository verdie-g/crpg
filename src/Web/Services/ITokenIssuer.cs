using System.Security.Claims;

namespace Trpg.Web.Services
{
    public interface ITokenIssuer
    {
        string IssueToken(ClaimsIdentity claimsIdentity);
    }
}