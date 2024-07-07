using MangaApplication.DTOs;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MangaApplication.Services;


public class TagRepository : ITagRepository
{
    // private const string DatabaseName = "MangaDatabase";
    private const string CollectionName = "tag";
    private readonly IMongoCollection<Entities.Tag> tagCollection;
    private readonly FilterDefinitionBuilder<Entities.Tag> filterBuilder = Builders<Entities.Tag>.Filter;
    public TagRepository(IMongoDatabase database)
    {
        // IMongoDatabase database = mongoClient.GetDatabase(DatabaseName);
        tagCollection = database.GetCollection<Entities.Tag>(CollectionName);
    }

    public async Task CreateTag(Entities.Tag tag)
    {
        await tagCollection.InsertOneAsync(tag);
    }

    public async Task DeleteTag(Guid id)
    {
        var filter = filterBuilder.Eq(tag => tag.Id, id);
        await tagCollection.DeleteOneAsync(filter);
    }

    public async Task<IEnumerable<Entities.Tag>> GetAllTags()
    {
        return await tagCollection.Find(new BsonDocument()).ToListAsync();
    }

    public async Task<Entities.Tag> GetTagById(Guid id)
    {
        var filter = filterBuilder.Eq(tag => tag.Id, id);
        return await tagCollection.Find(filter).SingleOrDefaultAsync();
    }

    public async Task UpdateTag(Entities.Tag tag)
    {
        var filter = filterBuilder.Eq(tag => tag.Id, tag.Id);
        await tagCollection.ReplaceOneAsync(filter, tag);
    }
}