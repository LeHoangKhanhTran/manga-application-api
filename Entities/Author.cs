using System;
using MangaApplication.DTOs;

namespace MangaApplication.Entities 
{
    public record AuthorManga(Guid MangaId, string MangaTitle);
    public class Author 
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public IEnumerable<Guid> Works { get; init; }
        public string Biography { get; init; }
        public string AvatarUrl { get; init; }
        
    }
}