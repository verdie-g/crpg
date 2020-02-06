using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Trpg.Web.Services
{
    public class JwtTokenIssuer : ITokenIssuer
    {
        private readonly SigningCredentials _signingCredentials;
        private readonly TimeSpan _lifetime;

        public JwtTokenIssuer(IOptions<JwtConfiguration> jwtConfig)
        {
            _signingCredentials = ComputeSigningCredentials(jwtConfig.Value.Secret);
            _lifetime = jwtConfig.Value.Lifetime;
        }

        public string IssueToken(ClaimsIdentity claimsIdentity)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = DateTime.UtcNow.Add(_lifetime),
                SigningCredentials = _signingCredentials,
            };

            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        private SigningCredentials ComputeSigningCredentials(string secret)
        {
            byte[] secretBytes = Encoding.ASCII.GetBytes(secret);
            var key = new SymmetricSecurityKey(secretBytes);
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
        }
    }

    public class JwtConfiguration
    {
        public string Secret { get; set; }
        public TimeSpan Lifetime { get; set; }
    }
}