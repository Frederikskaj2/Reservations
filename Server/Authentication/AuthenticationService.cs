using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Frederikskaj2.Reservations.Server;

public class AuthenticationService
{
    public const int TokenSigningKeyLength = 32;

    readonly byte[] accessTokenSigningKey;
    readonly TokensOptions options;
    readonly JwtSecurityTokenHandler tokenHandler = new();

    public AuthenticationService(IOptionsSnapshot<TokensOptions> options)
    {
        this.options = options.Value;

        accessTokenSigningKey = Convert.FromBase64String(this.options.AccessTokenSigningKey ?? "");
        if (accessTokenSigningKey.Length is not TokenSigningKeyLength)
            throw new ConfigurationException("Invalid access token signing key length.");
    }

    public Tokens CreateTokens(AuthenticatedUser authenticatedUser)
        => new(CreateAccessToken(authenticatedUser));

    string CreateAccessToken(AuthenticatedUser authenticatedUser)
    {
        var claims = GetClaims();
        var expireAt = authenticatedUser.Timestamp.Plus(options.AccessTokenLifetime);
        return CreateToken(claims, expireAt, accessTokenSigningKey);

        IEnumerable<Claim> GetClaims()
        {
            yield return new Claim(ClaimTypes.Email, authenticatedUser.Email.ToString());
            yield return new Claim(ClaimTypes.Name, authenticatedUser.FullName);
            yield return new Claim(ClaimTypes.NameIdentifier, authenticatedUser.UserId.ToString());
            foreach (var role in Enum.GetValues<Roles>().Where(r => authenticatedUser.Roles.HasFlag(r)))
                yield return new Claim(ClaimTypes.Role, role.ToString());
        }
    }

    string CreateToken(IEnumerable<Claim> claims, Instant expires, byte[] tokenSigningKey)
    {
        var key = new SymmetricSecurityKey(tokenSigningKey);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            options.Issuer,
            options.Audience,
            claims,
            expires: expires.ToDateTimeUtc(),
            signingCredentials: credentials);
        return tokenHandler.WriteToken(token);
    }
}
