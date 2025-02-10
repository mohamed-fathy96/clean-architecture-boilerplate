namespace CleanArchitecture.Application.Models;

public class JwtOptions
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public bool ValidateIssuer { get; set; }
    public bool ValidateAudience { get; set; }
    public bool ValidateLifeTime { get; set; }
    public TimeSpan TokenLifeTime { get; set; }
    public TimeSpan RefreshTokenLifeTime { get; set; }
}
