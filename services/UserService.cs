using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
using RequestsApi.Data;
using RequestsApi.Models;

namespace RequestsApi.services;

public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(MongoDbService mongoDbService)
    {
        _users = mongoDbService.GetCollection<User>("Users");
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
    }

    public async Task CreateUserAsync(User user)
    {
        user.Password = HashPassword(user.Password);
        user.Id = null;
        await _users.InsertOneAsync(user);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
