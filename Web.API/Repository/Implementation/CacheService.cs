using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
using WEB.API.Repository.Interfaces;

namespace WEB.API.Repository.Implementation
{
    public class CacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;

        public CacheService(IConnectionMultiplexer redis)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        }
        // соединение с сервером redis

        /// <summary>
        /// Получить объект из кэша по ключу.
        /// </summary>
        public async Task<T?> GetAsync<T>(string key)
        {
            var db = _redis.GetDatabase();
            var cachedValue = await db.StringGetAsync(key);

            if (!cachedValue.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(cachedValue);
        }

        /// <summary>
        /// Сохранить объект в кэше с указанием времени жизни.
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var db = _redis.GetDatabase();
            var jsonValue = JsonSerializer.Serialize(value);

            await db.StringSetAsync(key, jsonValue, expiration);
        }

        /// <summary>
        /// Удалить объект из кэша по ключу.
        /// </summary>
        public async Task<bool> RemoveAsync(string key)
        {
            var db = _redis.GetDatabase();
            return await db.KeyDeleteAsync(key);
        }

        /// <summary>
        /// Генерация уникального хэшированного ключа для кэша.
        /// </summary>
        /// <param name="methodName">Имя метода.</param>
        /// <param name="parameters">Параметры запроса.</param>
        /// <returns>Хэшированный ключ.</returns>
        public static string GenerateCacheKey(string methodName, params object[] parameters)
        {
            // Формирование строки для хэширования.
            var rawKey = $"{methodName}:{string.Join(":", parameters.Select(p => p.ToString()))}";

            // Генерация SHA256-хэша.
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawKey));

            // Преобразование хэша в строку.
            return Convert.ToBase64String(hashBytes);
        }
    }
}
