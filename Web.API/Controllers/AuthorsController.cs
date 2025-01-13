using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEB.API.DTO;
using WEB.API.Models; // Используем Authors из Models
using WEB.API.Repository.Implementation;
using WEB.API.Repository.Interfaces;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorsRepository _repository; // Интерфейс репозитория авторов
        private readonly ICacheService _cacheService; // Сервис кэширования
        private readonly IConfiguration _configuration; // Конфигурация приложения

        // Конструктор с внедрением зависимостей
        public AuthorsController(IAuthorsRepository repository, ICacheService cacheService, IConfiguration configuration)
        {
            _repository = repository;
            _cacheService = cacheService;
            _configuration = configuration;
        }

        // Метод для получения всех авторов с кэшированием
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Проверяем состояние модели
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Генерация ключа для кэша
            var cacheKey = CacheService.GenerateCacheKey(nameof(GetAll) + "Author");

            // Проверяем, включено ли кэширование
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                // Попытка получить данные из кэша
                var cachedAuthors = await _cacheService.GetAsync<IEnumerable<Authors>>(cacheKey);
                if (cachedAuthors != null)
                {
                    // Если данные найдены в кэше, возвращаем их
                    return Ok(cachedAuthors);
                }
            }

            // Если данных в кэше нет, получаем их из репозитория
            var authors = await _repository.GetAllAsync();

            // Сохраняем результат в кэш, если кэширование включено
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
                await _cacheService.SetAsync(cacheKey, authors, cacheTimeout);
            }

            // Возвращаем список авторов
            return Ok(authors);
        }

        // Метод для получения автора по id с кэшированием
        [HttpGet("getauthor/{id:int}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            // Проверяем состояние модели
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Генерация ключа для кэша
            var cacheKey = CacheService.GenerateCacheKey(nameof(Get) + "Author", id);

            // Проверяем, включено ли кэширование
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                // Попытка получить данные из кэша
                var cachedAuthor = await _cacheService.GetAsync<Authors>(cacheKey);
                if (cachedAuthor != null)
                {
                    // Если данные найдены в кэше, возвращаем их
                    return Ok(cachedAuthor);
                }
            }

            // Если данных в кэше нет, получаем их из репозитория
            var author = await _repository.GetAsync(id);
            if (author == null)
            {
                // Если автор не найден, возвращаем 404
                return NotFound(new { Message = "Автор с указанным id не найден." });
            }

            // Сохраняем результат в кэш, если кэширование включено
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
                await _cacheService.SetAsync(cacheKey, author, cacheTimeout);
            }

            // Возвращаем данные автора
            return Ok(author);
        }

        // Метод для удаления автора
        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            // Проверяем состояние модели
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Удаляем автора из репозитория
            bool deleted = await _repository.DeleteAsync(id);

            // Если автор не найден, возвращаем 404
            if (!deleted)
            {
                return NotFound(new { Message = "Автор с указанным id не найден." });
            }

            // Возвращаем успешный результат
            return Ok(new { Message = "Автор успешно удален." });
        }

        // Метод для создания нового автора
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] AuthorsCreateDTO createDTO)
        {
            // Проверяем состояние модели
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Создаем нового автора с использованием репозитория
            var createdAuthor = await _repository.CreateAsync(createDTO);

            // Возвращаем успешный ответ с созданным объектом
            return CreatedAtAction(nameof(Get), new { id = createdAuthor.Id }, createdAuthor);
        }

        // Метод для обновления данных автора
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] AuthorsUpdateDTO updateDTO)
        {
            // Проверяем состояние модели
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Обновляем автора с использованием репозитория
            var updatedAuthor = await _repository.UpdateAsync(id, updateDTO);

            // Если автор не найден, возвращаем 404
            if (updatedAuthor == null)
            {
                return NotFound(new { Message = "Автор с указанным id не найден." });
            }

            // Возвращаем успешный результат с обновленными данными
            return Ok(updatedAuthor);
        }
    }
}
