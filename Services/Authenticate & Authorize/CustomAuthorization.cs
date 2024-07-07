using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace MangaApplication.Services;

public class HttpOnlyCookieJwtRequirement : IAuthorizationRequirement {}
public class CustomAuthorizationHandler : AuthorizationHandler<HttpOnlyCookieJwtRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HttpOnlyCookieJwtRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var jwtToken = httpContext.Request.Cookies["access_token"] is not null ? httpContext.Request.Cookies["access_token"] : GetTokenInBearer();
        if (!string.IsNullOrEmpty(jwtToken) && HasAdministratorRole(jwtToken))
        {
            context.Succeed(requirement);
        }
        else
        {
            Console.WriteLine("B");
            context.Fail();
        }
        
        return Task.CompletedTask;

    }

    private string GetTokenInBearer()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            var headerValue = authorizationHeader.FirstOrDefault();
            if (headerValue is not null && headerValue.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                return headerValue.Split(' ')[1];
            }
        }
        return null;
    }

    private bool HasAdministratorRole(string jwtToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadToken(jwtToken) as JwtSecurityToken;
        var roles = token.Claims.Where(c => c.Type == "role").Select(c => c.Value);
        return roles.Contains("Administrator");
    }
}