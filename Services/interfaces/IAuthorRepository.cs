using MangaApplication.Entities;

namespace MangaApplication.Services
{
    public interface IAuthorRepository 
    {
        Task<IEnumerable<Author>> GetAllAuthors();
        Task<Author> GetAuthorById(Guid id);
        Task CreateAuthor(Author entity);
        Task UpdateAuthor(Author author);
        Task DeleteAuthor(Guid id);
        Task<IEnumerable<Author>> SearchAuthorsByName(string name);
        Task AddMangaToAuthor(Author author, Guid mangaId);
        Task RemoveMangaFromAuthor(Author author, Guid mangaId);
    }
}