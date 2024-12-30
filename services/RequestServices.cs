using MongoDB.Bson;
using MongoDB.Driver;
using RequestsApi.Data;
using RequestsApi.Models;
using StackExchange.Redis;

namespace RequestsApi.services
{
    public class RequestServices
    {
        private readonly IMongoCollection<Request> _requests;
        private readonly IDatabase _redisDatabase;
        private const string CacheKey = "requests";

        public RequestServices(MongoDbService mongoDbService, IDatabase redisDatabase)
        {
            _redisDatabase = redisDatabase;
            _requests = mongoDbService.GetCollection<Request>("Requests");
        }

        public async Task<List<Request>> GetAllAsync()
        {
            var cachedRequests = await _redisDatabase.StringGetAsync(CacheKey);

            if (!cachedRequests.IsNullOrEmpty)
            {
                var requests = System.Text.Json.JsonSerializer.Deserialize<List<Request>>(cachedRequests);
                if (requests != null)
                {
                    return requests;
                }
            }
            
            var requestsFromDb = await _requests.Find(_ => true).ToListAsync();
            
            var serializedRequests = System.Text.Json.JsonSerializer.Serialize(requestsFromDb);
            await _redisDatabase.StringSetAsync(CacheKey, serializedRequests, TimeSpan.FromMinutes(30));

            return requestsFromDb;
        }

        public async Task<Request?> GetByIdAsync(string id) =>
            await _requests.Find(item => item.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Request request)
        {
            request.Id = null;
            request.CreatedAt = DateTime.UtcNow;
            await _requests.InsertOneAsync(request);
            await InvalidateCache();
        }
        
        public async Task UpdateAsync(Request updatedRequest, string requestId)
        {
            if (!ObjectId.TryParse(requestId, out _))
            {
                throw new ArgumentException("Invalid ID format.");
            }

            var existingRequest = await _requests.Find(r => r.Id == requestId).FirstOrDefaultAsync();
            if (existingRequest == null)
            {
                throw new KeyNotFoundException($"Request with Id '{requestId}' not found.");
            }

            updatedRequest.Id = requestId;
            updatedRequest.CreatedAt = existingRequest.CreatedAt;
            updatedRequest.UpdatedAt = DateTime.UtcNow;

            await _requests.ReplaceOneAsync(r => r.Id == requestId, updatedRequest);
            await InvalidateCache();
        }
        
        public async Task DeleteAsync(string id)
        {
            var request = await _requests.Find(request => request.Id == id).FirstOrDefaultAsync();
            if (request == null)
            {
                throw new KeyNotFoundException($"Request with Id '{id}' not found.");
            }

            await _requests.DeleteOneAsync(r => r.Id == id);
            await InvalidateCache();
        }

        public async Task UpdateStatusAsync(string id, string status)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                throw new ArgumentException("Invalid ID format.");
            }
            
            var existingRequest = await _requests.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (existingRequest == null)
            {
                throw new KeyNotFoundException($"Request with Id '{id}' not found.");
            }
            
            var update = Builders<Request>.Update
                .Set(r => r.Status, status)
                .Set(r => r.UpdatedAt, DateTime.UtcNow);

            await _requests.UpdateOneAsync(r => r.Id == id, update);
            await InvalidateCache();
        }

        private async Task InvalidateCache()
        {
            await _redisDatabase.KeyDeleteAsync(CacheKey);
        }
    }
}