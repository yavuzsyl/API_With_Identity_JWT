using Identity.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Identity.Security.Token
{
    public class TokenHandler : ITokenHandler
    {
        private readonly AppTokenOptions tokenOptions;
        public TokenHandler(IOptions<AppTokenOptions> options)
        {
            tokenOptions = options.Value;
        }
        public AccessToken CreateAccessToken(AppUser user)
        {
            var accessTokenExpiration = DateTime.Now.AddMinutes(tokenOptions.AccessTokenExpiration);
            var securityKey = SignHandler.GetSecurityKey(tokenOptions.SecurityKey);
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            JwtSecurityToken jwtToken = new JwtSecurityToken(

                issuer: tokenOptions.Issuer,
                audience: tokenOptions.Audience,
                expires: accessTokenExpiration,
                notBefore: DateTime.Now,
                signingCredentials: credentials,
                claims: GetClaims(user)
                );

            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(jwtToken);

            return new AccessToken() { Token = token, Expiration = accessTokenExpiration, RefreshToken = CreateRefreshToken() };
        }

        private string CreateRefreshToken()
        {
            var numberBytes = new Byte[32];
            using (var rng =  RandomNumberGenerator.Create())
            {
                rng.GetBytes(numberBytes);
                return Convert.ToBase64String(numberBytes);
            }
        }

        //token payloadu
        private IEnumerable<Claim> GetClaims(AppUser user)
        {
            return new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(JwtRegisteredClaimNames.Email,user.Email),
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())

            };
        }
        //şimdilik boş user class'a refreshtoken atanmalı
        public void RevokeRefreshToken(AppUser user)
        {
            throw new NotImplementedException();
        }
    }
}
