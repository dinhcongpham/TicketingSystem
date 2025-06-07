using PaymentService.Data;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using PaymentService.Entities;

namespace PaymentService.Infrastructure.Persistence
{
    public class PaymentContext
    {
        private readonly IMongoDatabase _database;

        public PaymentContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.Database);
        }

        public IMongoCollection<Payment> Payments => _database.GetCollection<Payment>("Payments");
    }
}
