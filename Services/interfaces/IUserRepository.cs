using MangaApplication.Entities;

namespace MangaApplication.Services;
public interface IUserRepository 
{
    public Task<User> GetUserById(Guid id);
    public Task<User> GetUserByUsername(string username);
    public Task<User> GetUserByEmail(string email);
    public Task CreateUser(User user);
    public Task UpdateUser(User user);
}