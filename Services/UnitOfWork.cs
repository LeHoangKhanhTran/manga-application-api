using MangaApplication.Entities;
using MangaApplication.Services.Repositories;
using MongoDB.Driver;
// using MangaApplication.Services.Repositories.AuthorRepository;
namespace MangaApplication.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private const string databaseName = "MangaDatabase";
        private readonly IMongoDatabase database;
        public IMangaRepository mangaRepository { get; }
        public IAuthorRepository authorRepository { get; }
        public ITagRepository tagRepository { get; }
        public UnitOfWork(IMongoClient mongoClient)
        {
            database = mongoClient.GetDatabase(databaseName);
            this.mangaRepository = new MangaRepository(database);
            this.authorRepository = new AuthorRepository(database);
            this.tagRepository = new TagRepository(database);
        }
    }
}