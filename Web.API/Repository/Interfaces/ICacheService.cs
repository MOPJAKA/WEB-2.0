using System;
using System.Threading.Tasks;

namespace WEB.API.Repository.Interfaces
{
    public interface ICacheService
    {
        /// <summary>
        /// Получить объект из кэша по ключу.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="key">Ключ.</param>
        /// <returns>Объект типа T или null, если объект отсутствует в кэше.</returns>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Сохранить объект в кэше с указанием времени жизни.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="key">Ключ.</param>
        /// <param name="value">Объект.</param>
        /// <param name="expiration">Время жизни объекта.</param>
        /// <returns>Задача без возвращаемого значения.</returns>
        Task SetAsync<T>(string key, T value, TimeSpan expiration);

        /// <summary>
        /// Удалить объект из кэша по ключу.
        /// </summary>
        /// <param name="key">Ключ.</param>
        /// <returns>True, если объект успешно удалён, иначе false.</returns>
        Task<bool> RemoveAsync(string key);
    }
}
