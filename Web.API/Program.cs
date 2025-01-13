using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Core; // Для HttpProtocols
using WEB.API.Context;
using WEB.API.Repository.Interfaces;
using WEB.API.Repository.Implementation;
using WEB.API;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis; // Подключаем Redis

var builder = WebApplication.CreateBuilder(args);
// Используется для конфигурации приложения, добавления сервисов и настройки различных параметров

// Настройка Kestrel для прослушивания gRPC на порту 5001
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, o =>
    {
        o.Protocols = HttpProtocols.Http2; // gRPC требует HTTP/2
        o.UseHttps(); // Используйте HTTPS для gRPC (шифрование)
    });
});

// Подключение конфигурации из appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Получаем строку подключения к PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Регистрация DbContext для PostgreSQL
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseNpgsql(connectionString));  // PostgreSQL

// Регистрация репозиториев
builder.Services.AddScoped<IBooksRepository, BooksRepository>();
builder.Services.AddScoped<IAuthorsRepository, AuthorsRepository>();
builder.Services.AddScoped<IReadersRepository, ReadersRepository>();
builder.Services.AddScoped<IPublishersRepository, PublishersRepository>();
builder.Services.AddScoped<IBookIssuesRepository, BookIssuesRepository>();

// Регистрация кэш-сервиса и конфигурация Redis
builder.Services.AddSingleton<ICacheService, CacheService>();

// Регистрируем Redis IConnectionMultiplexer
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");
    return ConnectionMultiplexer.Connect(redisConnectionString);
});

// Регистрация контроллеров (REST API)
builder.Services.AddControllers();

// Добавление Swagger/OpenAPI для документации
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Добавление gRPC сервисов
builder.Services.AddGrpc();

// Проверяем, какие API модули включены
var enableRestApi = builder.Configuration.GetValue<bool>("ApiSettings:EnableRestApi");
var enableGrpcApi = builder.Configuration.GetValue<bool>("ApiSettings:EnableGrpcApi");

// Строим приложение после всех регистраций
var app = builder.Build();

// Настройка Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Подключаем REST API, если он включен
if (enableRestApi)
{
    app.MapControllers();
    // подключение всех маршрутов (например, GET/api/Authors)
}

// Подключение gRPC API, если он включен
if (enableGrpcApi)
{
    app.MapGrpcService<AuthorsServiceImpl>();
    app.MapGrpcService<BooksServiceImpl>();
    app.MapGrpcService<PublishersServiceImpl>();
    app.MapGrpcService<ReadersServiceImpl>();
    app.MapGrpcService<BookIssuesServiceImpl>();
}

// Запускаем приложение
app.Run();
