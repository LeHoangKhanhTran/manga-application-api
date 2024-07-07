using System.ComponentModel.DataAnnotations;
using MangaApplication.Entities;
namespace MangaApplication.DTOs 
{
    public record MangaDto(Guid Id, string Title, string Summary, MangaAuthorDto Author, string Status, string ImageUrl, IEnumerable<Guid> TagIds, decimal Rating, DateTimeOffset CreatedDate, int Follows);
    public record CreateMangaDto([Required] string Title, string Summary, Guid authorId, [CheckStatus(ErrorMessage = "Invalid Status")]string Status, IFormFile? Image, IEnumerable<Guid> TagIds);
    public record UpdateMangaDto(string? Title, string? Summary, Guid? authorId, [CheckStatus(ErrorMessage = "Invalid Status")]string? Status, IFormFile? Image,  IEnumerable<Guid>? TagIds);
    public record MangaAuthorDto(Guid AuthorId, string AuthorName);
    public record SearchMangaDto(Guid Id, string Title, string Status, string ImageUrl, decimal Rating, int Follows);
    public record RatingDto(Guid UserId, int Score, int PreviousScore);
    public record RemoveRatingDto(Guid UserId, int Score);
    public record FollowDto(Guid UserId);
}