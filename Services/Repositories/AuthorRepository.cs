using MangaApplication.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MangaApplication.Services.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        // private const string DatabaseName = "MangaDatabase";
        private const string CollectionName = "author";
        private IMongoCollection<Author> authorCollection;
        private FilterDefinitionBuilder<Author> filterBuilder = Builders<Author>.Filter;
        public AuthorRepository(IMongoDatabase database)
        {
            // IMongoDatabase database = mongoClient.GetDatabase(DatabaseName);
            authorCollection = database.GetCollection<Author>(CollectionName);
        }

        public async Task<IEnumerable<Author>> GetAllAuthors()
        {
            return await authorCollection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task CreateAuthor(Author entity)
        {
            await authorCollection.InsertOneAsync(entity);
        }

        public async Task DeleteAuthor(Guid id)
        {
            var filter = filterBuilder.Eq(author => author.Id, id);
            await authorCollection.DeleteOneAsync(filter);
        }

        public async Task<Author> GetAuthorById(Guid id)
        {
            var filter = filterBuilder.Eq(author => author.Id, id);
            return await authorCollection.Find(filter).SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<Author>> SearchAuthorsByName(string name)
        {
            var filter = filterBuilder.Regex(author => author.Name, new BsonRegularExpression($"(?i){name}"));
            return await authorCollection.Find(filter).ToListAsync();
        }

        public async Task UpdateAuthor(Author author)
        {
            var filter = filterBuilder.Eq(existingAuthor => existingAuthor.Id, author.Id);
            await authorCollection.ReplaceOneAsync(filter, author);
        }

        public async Task AddMangaToAuthor(Author author, Guid mangaId)
        {
            IEnumerable<Guid> newWorks = author.Works.Append(mangaId);
            Author author1 = new()
            {
                Id = author.Id,
                Name = author.Name,
                Works = newWorks,
                Biography = author.Biography,
                AvatarUrl = author.AvatarUrl
            };
            await UpdateAuthor(author1);
        }
        public async Task RemoveMangaFromAuthor(Author author, Guid mangaId)
        {
            if (author is not null && author.Works is not null)
            {
                List<Guid> worksList = author.Works.ToList();
                int index = worksList.IndexOf(mangaId);
                if (index != -1)
                {
                    worksList.RemoveAt(index);
                    Author author1 = new()
                    {
                        Id = author.Id,
                        Name = author.Name,
                        Works = worksList,
                        Biography = author.Biography,
                        AvatarUrl = author.AvatarUrl
                    };
                    await UpdateAuthor(author1);
                }
            }
        }
    }
}