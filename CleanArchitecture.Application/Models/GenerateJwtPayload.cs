using System.Security.Claims;

namespace CleanArchitecture.Application.Models;

public class GenerateJwtPayload
{
    public List<Claim> Claims { get; set; }
}
