namespace MangaApplication.Services;

public interface IUnitOfWork
{
    IMangaRepository mangaRepository { get; }
    IAuthorRepository authorRepository { get; }
    ITagRepository tagRepository { get; }
}