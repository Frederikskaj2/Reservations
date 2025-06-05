using Frederikskaj2.Reservations.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;

namespace Frederikskaj2.Reservations.Users;

class ConfigureJwtBearerOptions(IOptions<TokensOptions> tokensOptions) : IPostConfigureOptions<JwtBearerOptions>
{
    readonly TokensOptions tokensOptions = tokensOptions.Value;

    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        var accessTokenSigningKey = Convert.FromBase64String(tokensOptions.AccessTokenSigningKey ?? "");
        if (accessTokenSigningKey.Length is not AuthenticationService.TokenSigningKeyLength)
            throw new ConfigurationException("Invalid access token signing key length.");
        options.Authority = tokensOptions.Issuer;
        options.Audience = tokensOptions.Audience;
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = tokensOptions.Issuer,
            ValidAudience = tokensOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(accessTokenSigningKey),
            ClockSkew = tokensOptions.ClockSkew.ToTimeSpan(),
        };
        options.IncludeErrorDetails = true;
    }
}
