using MangaApplication.Entities;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Runtime.CompilerServices;
using MongoDB.Bson.Serialization;
namespace MangaApplication.Services.Repositories
{
    public class MangaRepository : IMangaRepository
    {
        // private const string DatabaseName = "MangaDatabase";
        private const string CollectionName = "manga";
        private IMongoCollection<Manga> mangaCollection;
        private readonly FilterDefinitionBuilder<Manga> filterBuilder = Builders<Manga>.Filter;
        public MangaRepository(IMongoDatabase database) {
            // IMongoDatabase database = mongoClient.GetDatabase(DatabaseName);
            mangaCollection = database.GetCollection<Manga>(CollectionName);
        }

         public async Task<IEnumerable<Manga>> GetAllManga()
        {
            return await mangaCollection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task<Manga> GetMangaByID(Guid id)
        { 
            var filter = filterBuilder.Eq(manga => manga.Id, id);
            return await mangaCollection.Find(filter).SingleOrDefaultAsync();
        }

        public async Task CreateManga(Manga manga)
        {
            await mangaCollection.InsertOneAsync(manga); 
        }

        public async Task UpdateManga(Manga manga)
        {
            var filter = filterBuilder.Eq(existingManga => existingManga.Id, manga.Id);
            await mangaCollection.ReplaceOneAsync(filter, manga);
        }
        
        public async Task DeleteManga(Guid id)
        {
            var filter = filterBuilder.Eq(manga => manga.Id, id);
            await mangaCollection.DeleteOneAsync(filter);
        }    

        public async Task<IEnumerable<Manga>> SearchMangaByTitle(string title)
        {
            var filter = filterBuilder.Regex(manga => manga.Title, new BsonRegularExpression($"(?i){title}"));
            return await mangaCollection.Find(filter).ToListAsync();
        }

        public async Task<Manga> GetRandomManga() 
        {
            var x = await mangaCollection.CountDocumentsAsync(FilterDefinition<Manga>.Empty);
            Random random = new();
            var index = random.Next(0, (int)x);
            var projection = Builders<Manga>.Projection.Include(manga => manga.Id);
            return await mangaCollection.Find(Builders<Manga>.Filter.Empty).Skip(index).Limit(1).Project<Manga>(projection).FirstAsync();
        }

        public async IAsyncEnumerable<Manga> GetMangaList(int numberOfItem) 
        {
           var count = await mangaCollection.CountDocumentsAsync(FilterDefinition<Manga>.Empty);
           List<int> indexList = CustomRandom.YieldUniqueNumbers(numberOfItem, (int)count);
           for (int i = 0; i < numberOfItem; i++) {
             yield return await mangaCollection.Find(Builders<Manga>.Filter.Empty).Skip(indexList[i]).Limit(1).FirstOrDefaultAsync();
           }
        }

        public async Task<IEnumerable<Manga>> GetMangaListByDate(int numberOfItem) 
        {
            var sort = Builders<Manga>.Sort.Descending("CreatedDate");
            return await mangaCollection.Find(Builders<Manga>.Filter.Empty).Sort(sort).Limit(numberOfItem).ToListAsync();
        }

        public async Task<IEnumerable<Manga>> GetTopRatedManga(int numberOfItem) 
        {

            var sort = Builders<Manga>.Sort.Descending(manga => manga.Rating.AverageScore);
            var result =  await mangaCollection.Find(Builders<Manga>.Filter.Empty).Sort(sort).Limit(numberOfItem).ToListAsync();
            return result;
        }
        
}
    }
