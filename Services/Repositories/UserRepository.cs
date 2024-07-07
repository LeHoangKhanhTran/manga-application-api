using MongoDB.Driver;
using MangaApplication.Entities;
using Microsoft.AspNetCore.Mvc;
using MangaApplication.DTOs;
namespace MangaApplication.Services;

public class UserRepository : IUserRepository
{
    private const string databaseName = "MangaDatabase";
    private const string collectionName = "user";
    private readonly IMongoCollection<User> userCollection;
    private readonly FilterDefinitionBuilder<User> filterBuilder = Builders<User>.Filter;
    public UserRepository(IMongoClient mongoClient)
    {
        this.userCollection = mongoClient.GetDatabase(databaseName).GetCollection<User>(collectionName);
    }

    public async Task<User> GetUserById(Guid id)
    {
        var filter = filterBuilder.Eq(user => user.Id, id);
        return await userCollection.Find(filter).FirstOrDefaultAsync();
    }
    public async Task<User> GetUserByUsername(string username)
    {
        var filter = filterBuilder.Eq(user => user.Username, username);
        return await userCollection.Find(filter).FirstOrDefaultAsync();
    }
    public async Task<User> GetUserByEmail(string email)
    {
        var filter = filterBuilder.Eq(user => user.Email, email);
        return await userCollection.Find(filter).FirstOrDefaultAsync();
    }
    public async Task CreateUser(User user)
    {
        await userCollection.InsertOneAsync(user);
    }

    public async Task UpdateUser(User user) 
    {
        var filter = filterBuilder.Eq(existingUser => existingUser.Id, user.Id);
        await userCollection.ReplaceOneAsync(filter, user);
    }

}