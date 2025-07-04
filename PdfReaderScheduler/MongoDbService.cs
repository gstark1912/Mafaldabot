using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text;

public class MongoDbService
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<MongoDbService> _logger;

    public MongoDbService(ILogger<MongoDbService> logger)
    {
        _logger = logger;
        var client = new MongoClient("mongodb://localhost:27018");
        _database = client.GetDatabase("PdfReaderDb");
    }

    public async Task<ReadingState?> GetReadingStateAsync()
    {
        try
        {
            var collection = _database.GetCollection<ReadingState>("ReadingState");
            var state = await collection.Find(Builders<ReadingState>.Filter.Empty).FirstOrDefaultAsync();

            if (state != null)
            {
                _logger.LogInformation("Estado de lectura obtenido: Página actual {CurrentPage}, Última ejecución {LastRun}",
                    state.CurrentPage, state.LastRunDateTime);
            }
            else
            {
                _logger.LogWarning("No se encontró estado de lectura en MongoDB");
            }

            return state;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estado de lectura desde MongoDB");
            return null;
        }
    }

    public async Task UpdateLastRunDateTimeAsync(ObjectId id, DateTime lastRunDateTime)
    {
        try
        {
            var collection = _database.GetCollection<ReadingState>("ReadingState");
            var filter = Builders<ReadingState>.Filter.Eq(x => x.Id, id);
            var update = Builders<ReadingState>.Update.Set(x => x.LastRunDateTime, lastRunDateTime);

            await collection.UpdateOneAsync(filter, update);
            _logger.LogInformation("Fecha de última ejecución actualizada a {LastRun}", lastRunDateTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar fecha de última ejecución en MongoDB");
        }
    }
}