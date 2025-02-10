using System.Security.Claims;
using CleanArchitecture.Application.Models;
using CleanArchitecture.Domain.Shared.Dtos.Identity;

namespace CleanArchitecture.Application.Services.JwtServiceProvider;

public interface IJwtServiceProvider
{
    JwtResultDto GenerateToken(GenerateJwtPayload payload);
    List<Claim> GetClaimsFromToken(string token);
    JwtValidationResultDto ValidateJwt(string jwtString);
}
