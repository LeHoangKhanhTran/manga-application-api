using MangaApplication.DTOs;
using MangaApplication.Entities;
using MangaApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace MangaApplication.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserRepository userRepository;
    private Authenticator authenticator;
    private readonly IImageUploader imageUploader;
    private readonly ILogger<UserController> logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public UserController(IConfiguration config, IUserRepository userRepository, IImageUploader imageUploader, ILogger<UserController> logger, IHttpContextAccessor httpContextAccessor)
    {
        this.userRepository = userRepository;
        this.authenticator = new Authenticator(config, userRepository);
        this.imageUploader = imageUploader;
        this.logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("authenticate")]
    public async Task<ActionResult> Login([FromBody] UserLoginDto userDto)
    {
        if (userDto.UsernameOrEmail is null || userDto.UsernameOrEmail.Length == 0) throw new Exception("No username nor email was provided.");
        var token = await authenticator.Authenticate(userDto.UsernameOrEmail, userDto.Password);
        if (token is null)
            return Unauthorized();
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        };
        this.HttpContext.Response.Cookies.Append("access_token", token, cookieOptions);
        return Ok(new {token});
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("register")]
    public async Task<ActionResult<User>> Register(CreateUserDto userDto)
    {
        if ((userDto.Username is null || userDto.Username.Length == 0) && (userDto.Email is null || userDto.Email.Length == 0)) throw new Exception("No username nor email was provided.");
        if (userDto.Email is not null && userDto.Email.Length > 0)  
        {
            if (!Validator.IsValidEmail(userDto.Email))
            {
                throw new Exception("Invalid email.");
            }
        }
        if (userDto.Username != null) 
        {
            var existingUser = await userRepository.GetUserByUsername(userDto.Username);
            if (existingUser is not null) throw new Exception("Username already exists.");
        }
        if (userDto.Email != null) 
        {
            var existingUser = await userRepository.GetUserByEmail(userDto.Email);
            if (existingUser is not null) throw new Exception("Email already exists.");
        }
        Security.CheckValidPassword(userDto.Password);
        string avatarUrl = "";
        // try 
        // {
        //     if (userDto.Avatar is not null)
        //     {
        //         var uploadResult = await imageUploader.UploadImage(userDto.Avatar, "user");
        //         avatarUrl = uploadResult.Url.ToString();
        //     }
        // }
        // catch(Exception e)
        // {
        //     logger.LogError(e, e.Message);
        // }
        User user = new()
        {
            Id = Guid.NewGuid(),
            Username = userDto.Username != null && userDto.Username.Length > 0 ? userDto.Username : null,
            Password = Security.HashPassword(userDto.Password),
            Email = userDto.Email != null && userDto.Email.Length > 0 ? userDto.Email : null,
            AvatarUrl = avatarUrl,
            Role = userDto.Role,
            MangaFollows = new List<Guid>(),
            UserRatings = new List<UserRating>()
        };
        await userRepository.CreateUser(user);
        logger.LogInformation("New user registered.");
        return Ok();
    }

    [HttpGet]
    [Route("me")]
    public async Task<ActionResult<UserDto>> GetUser()
    {
        string token = HttpContext.Request.Cookies["access_token"];
        if (token is null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
            {
                var headerValue = authorizationHeader.FirstOrDefault();
                if (headerValue is not null && headerValue.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase))
                {
                    token = headerValue.Split(' ')[1];
                }
            }
            else 
            {
                return Unauthorized();
            }
        }
        var principal = authenticator.GetClaimsFromToken(token);
        
        if (principal != null)
        {
            var username = principal.FindFirst(ClaimTypes.Name)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var user = username is not null ? await userRepository.GetUserByUsername(username) : await userRepository.GetUserByEmail(email);
            return user.AsDto();
        }
        logger.LogInformation($"Return the information of user whose token is {token}");
        return Unauthorized();
    }

    [HttpPost]
    [Route("logout")]
    public async Task<ActionResult> LogOut() {
        var cookieOptions = new CookieOptions 
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Domain = "localhost", 
            Expires = DateTime.UtcNow.AddDays(-1)
        };
        Response.Cookies.Append("access_token", "", cookieOptions);
        return Ok(new {message = "User logged out"});
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser(Guid id, [FromForm] UpdateUserDto userDto) 
    {
        var existingUser = await userRepository.GetUserById(id);
        if (existingUser is null)
        {
            return NotFound();
        }
        User user = new()
        {
            Id = existingUser.Id,
            Username = userDto.Username is not null && userDto.Username.Length > 0 ? userDto.Username : existingUser.Username,
            Password = userDto.Password is not null && userDto.Password.Length > 0 ? Security.HashPassword(userDto.Password) : existingUser.Password,
            Email = userDto.Email is not null && userDto.Email.Length > 0 ? userDto.Email : existingUser.Email,
            AvatarUrl = userDto.AvatarUrl is not null && userDto.AvatarUrl.Length > 0 ? userDto.AvatarUrl : existingUser.AvatarUrl,
            Role = existingUser.Role,
            MangaFollows = userDto.MangaFollows is not null ? userDto.MangaFollows : existingUser.MangaFollows,
            UserRatings = userDto.UserRatings is not null ? userDto.UserRatings : existingUser.UserRatings
        };
        await userRepository.UpdateUser(user);
        return NoContent();
    }
}