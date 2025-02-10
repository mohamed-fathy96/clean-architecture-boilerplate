using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CleanArchitecture.Application.Models;
using CleanArchitecture.Domain.Shared.Dtos.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CleanArchitecture.Application.Services.JwtServiceProvider;

public class JwtServiceProvider : IJwtServiceProvider
{
    private readonly JwtOptions _options;

    public JwtServiceProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public JwtResultDto GenerateToken(GenerateJwtPayload payload)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

        var claimSubject = new ClaimsIdentity();

        // Basic Claims
        claimSubject.AddClaims(new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString(CultureInfo.CurrentCulture))
        });

        // Custom Claims like roles and permissions
        if (payload.Claims is not null && payload.Claims.Count > 0)
        {
            claimSubject.AddClaims(payload.Claims);
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimSubject,
            Expires = DateTime.UtcNow.Add(_options.TokenLifeTime),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(token);

        return new JwtResultDto
        {
            Token = accessToken,
            Jti = token.Id
        };
    }

    public List<Claim> GetClaimsFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.ReadJwtToken(token).Claims.ToList();
    }

    public JwtValidationResultDto ValidateJwt(string jwtString)
    {
        var validationResult = new JwtValidationResultDto();
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Set the clock skew to zero for simplicity
            };

            var claimsPrincipal = tokenHandler.ValidateToken(jwtString, validationParameters, out var validatedToken);
            validationResult.IsValid = true;
            validationResult.IsExpired = validatedToken.ValidTo < DateTime.UtcNow;
            validationResult.Claims = claimsPrincipal.Claims.ToList();
            validationResult.Jti = validatedToken.Id;
        }
        catch (Exception)
        {
            // Token validation failed
            validationResult.IsValid = false;
            validationResult.IsExpired = false; // Not applicable since the token is not valid
            validationResult.Claims = null; // No claims to retrieve
        }

        return validationResult;
    }
}
