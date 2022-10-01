using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;

namespace Frederikskaj2.Reservations.Server;

class ConfigureJwtBearerOptions : IPostConfigureOptions<JwtBearerOptions>
{
    readonly TokensOptions tokensOptions;

    public ConfigureJwtBearerOptions(IOptions<TokensOptions> tokensOptions) => this.tokensOptions = tokensOptions.Value;

    public void PostConfigure(string name, JwtBearerOptions options)
    {
        var accessTokenSigningKey = Convert.FromBase64String(tokensOptions.AccessTokenSigningKey ?? "");
        if (accessTokenSigningKey.Length is not AuthenticationService.TokenSigningKeyLength)
            throw new ConfigurationException("Invalid access token signing key length.");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = tokensOptions.Issuer,
            ValidAudience = tokensOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(accessTokenSigningKey),
            ClockSkew = tokensOptions.ClockSkew.ToTimeSpan()
        };
        options.IncludeErrorDetails = true;
    }
}
