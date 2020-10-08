using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Crpg.WebApi.Services
{
    public class JwtTokenIssuer : ITokenIssuer
    {
        private readonly SigningCredentials _signingCredentials;
        private readonly TimeSpan _lifetime;

        public JwtTokenIssuer(JwtOptions jwtOptions)
        {
            _signingCredentials = ComputeSigningCredentials(jwtOptions.Secret);
            _lifetime = jwtOptions.Lifetime;
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

    public class JwtOptions
    {
        public const string Position = "Jwt";

        public string Secret { get; set; } = string.Empty;
        public TimeSpan Lifetime { get; set; }
    }
}
