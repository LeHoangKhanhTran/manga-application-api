using System.ComponentModel.DataAnnotations;

namespace MangaApplication.DTOs 
{
    public record AuthorDto(Guid Id, string Name, IEnumerable<Guid> Works, string Biography, string AvatarUrl);
    public record CreateAuthorDto([Required]string Name, string? Biography, IFormFile? Avatar);
    public record UpdateAuthorDto(string? Name, string? Biography, IFormFile? Avatar);
    public record AuthorMangaDto(Guid MangaId, string MangaTitle);
}