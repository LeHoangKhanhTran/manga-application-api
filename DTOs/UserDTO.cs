using MangaApplication.Entities;

namespace MangaApplication.DTOs;

public record UserLoginDto(string UsernameOrEmail, string Password);
public record UserDto(Guid Id, string Username, string Email, string AvatarUrl, string Role, IEnumerable<Guid> MangaFollows, IEnumerable<UserRating>? UserRatings);
public record CreateUserDto(string Username, string Password, string Email, string Role);
public record UpdateUserDto(string? Username, string? Password, string? Email, string? AvatarUrl, IEnumerable<Guid> MangaFollows, IEnumerable<UserRating> UserRatings);
