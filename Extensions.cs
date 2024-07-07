using MangaApplication.DTOs;
using MangaApplication.Entities;

namespace MangaApplication 
{
    public static class Extension 
    {
        public static MangaDto AsDto(this Manga manga) 
        {
            decimal rating = manga.Rating.AverageScore;
            return new MangaDto(manga.Id, manga.Title, manga.Summary, manga.Author.AsDto(), manga.Status, manga.ImageUrl, manga.TagIds, 
                                rating, manga.CreatedDate, manga.Follows);
        }

        public static SearchMangaDto AsSearchDto(this Manga manga) 
        {
            decimal rating = manga.Rating.AverageScore;
            return new SearchMangaDto(manga.Id, manga.Title, manga.Status, manga.ImageUrl, rating, manga.Follows);
        }
        public static AuthorDto AsDto(this Author author)
        {
            return new AuthorDto(author.Id, author.Name, author.Works, author.Biography, author.AvatarUrl);
        }

        public static MangaAuthorDto AsDto(this MangaAuthor author)
        {
            return new MangaAuthorDto(author.AuthorId, author.AuthorName);
        }

        // public static AuthorMangaDto AsDto(this AuthorManga manga)
        // {
        //     return new AuthorMangaDto(manga.MangaId, manga.MangaTitle);
        // }

        public static TagDto AsDto(this Tag tag)
        {
            return new TagDto(tag.Id, tag.Type, tag.Name);
        }

        public static UserDto AsDto(this User user)
        {
            return new UserDto(user.Id, user.Username, user.Email, user.AvatarUrl, user.Role, user.MangaFollows, user.UserRatings);
        }
    }
}