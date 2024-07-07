using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace MangaApplication.Services;

public class Authenticator
{
    private readonly IConfiguration config;
    private readonly IUserRepository userRepository;
    public Authenticator(IConfiguration config, IUserRepository userRepository)
    {
        this.config = config;
        this.userRepository = userRepository;
    }
    public async Task<string> Authenticate(string firstCredentials, string password)
    {
        var user = Validator.IsValidEmail(firstCredentials) ? await userRepository.GetUserByEmail(firstCredentials) : await userRepository.GetUserByUsername(firstCredentials);
        if (user is null) 
            throw new Exception("Can't find user with the provided username/email.");
        if (!Security.VerifyPassword(password, user.Password))
            throw new Exception("Incorrect password.");
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenKey = Encoding.ASCII.GetBytes(config["Authentication:JwtKey"].ToString());
        string type = Validator.IsValidEmail(firstCredentials) ? ClaimTypes.Email : ClaimTypes.Name;
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new Claim[] {
                new Claim(type, firstCredentials),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role is not null ? user.Role : "User")
            }),
            Expires = DateTime.UtcNow.AddHours(72),
            SigningCredentials = new SigningCredentials
            (
                new SymmetricSecurityKey(tokenKey),
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
       
    }

    public ClaimsPrincipal GetClaimsFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenKey = Encoding.ASCII.GetBytes(config["Authentication:JwtKey"].ToString());
        var validationParameters = new TokenValidationParameters 
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(tokenKey),
            ValidateIssuer = false,
            ValidateAudience = false
        };
        try 
        {
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);
            return principal;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
        
    }
}