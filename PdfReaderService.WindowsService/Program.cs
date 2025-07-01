using MongoDB.Driver;
using PdfReaderService.WindowsService;
using PdfReaderService.WindowsService.Services;

var builder = Host.CreateApplicationBuilder(args);

// MongoDB Configuration
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDB");
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration.GetValue<string>("MongoDB:DatabaseName");
    return client.GetDatabase(databaseName);
});

// HttpClient for API calls
builder.Services.AddHttpClient<IApiService, ApiService>();

// Register services
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddHostedService<Worker>();

// Windows Service support
builder.Services.AddWindowsService();

var host = builder.Build();
host.Run();
