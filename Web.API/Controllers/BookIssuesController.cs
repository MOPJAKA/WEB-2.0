using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEB.API.DTO;
using WEB.API.Models; // Используем BookIssues из Models
using WEB.API.Repository.Implementation;
using WEB.API.Repository.Interfaces;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookIssuesController : ControllerBase
    {
        private readonly IBookIssuesRepository _repository;
        private readonly ICacheService _cacheService;  // Redis caching service
        private readonly IConfiguration _configuration; // App configuration

        public BookIssuesController(IBookIssuesRepository repository, ICacheService cacheService, IConfiguration configuration)
        {
            _repository = repository;
            _cacheService = cacheService;
            _configuration = configuration;
        }

        // Method for getting all book issues with caching
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cacheKey = CacheService.GenerateCacheKey(nameof(GetAll) + "BookIssues");

            // Check if Redis caching is enabled
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                // Attempt to get data from the cache
                var cachedBookIssues = await _cacheService.GetAsync<IEnumerable<BookIssues>>(cacheKey);
                if (cachedBookIssues != null)
                {
                    return Ok(cachedBookIssues);  // Return cached data
                }
            }

            // If no data in cache, fetch from repository
            var bookIssues = await _repository.GetAllAsync();

            // Save result to cache if caching is enabled
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
                await _cacheService.SetAsync(cacheKey, bookIssues, cacheTimeout);
            }

            return Ok(bookIssues);  // Return fresh data
        }

        // Method for getting a specific book issue by id with caching
        [HttpGet("getbookissue/{id:int}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cacheKey = CacheService.GenerateCacheKey(nameof(Get) + "BookIssues", id);

            // Check if Redis caching is enabled
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                // Attempt to get data from the cache
                var cachedBookIssue = await _cacheService.GetAsync<BookIssues>(cacheKey);
                if (cachedBookIssue != null)
                {
                    return Ok(cachedBookIssue);  // Return cached data
                }
            }

            // If no data in cache, fetch from repository
            var bookIssue = await _repository.GetAsync(id);
            if (bookIssue == null)
            {
                return NotFound(new { Message = "Запись о выдаче книги с указанным id не найдена." });
            }

            // Save result to cache if caching is enabled
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
                await _cacheService.SetAsync(cacheKey, bookIssue, cacheTimeout);
            }

            return Ok(bookIssue);  // Return fresh data
        }

        // Method for getting overdue book issues with caching
        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdueIssues()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cacheKey = CacheService.GenerateCacheKey(nameof(GetOverdueIssues) + "BookIssues");

            // Check if Redis caching is enabled
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                // Attempt to get overdue issues data from the cache
                var cachedOverdueIssues = await _cacheService.GetAsync<IEnumerable<BookIssues>>(cacheKey);
                if (cachedOverdueIssues != null)
                {
                    return Ok(cachedOverdueIssues);  // Return cached data
                }
            }

            // If no data in cache, fetch overdue issues from repository
            var overdueIssues = await _repository.GetOverdueIssuesAsync();

            if (overdueIssues == null || !overdueIssues.Any())
            {
                return NotFound(new { Message = "Нет просроченных выдач книг." });
            }

            // Save the overdue issues data to the cache if Redis caching is enabled
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
                await _cacheService.SetAsync(cacheKey, overdueIssues, cacheTimeout);
            }

            return Ok(overdueIssues);  // Return the fresh data
        }

        // Method for getting book issues by reader id with caching
        [HttpGet("reader")]
        public async Task<IActionResult> GetByReaderId([FromQuery] int? readerId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (readerId == null)
            {
                return BadRequest(new { Message = "Не указан идентификатор читателя." });
            }

            var cacheKey = CacheService.GenerateCacheKey(nameof(GetByReaderId) + "BookIssues", readerId);

            // Check if Redis caching is enabled
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                // Attempt to get data from the cache
                var cachedBookIssues = await _cacheService.GetAsync<IEnumerable<BookIssues>>(cacheKey);
                if (cachedBookIssues != null)
                {
                    return Ok(cachedBookIssues);  // Return cached data
                }
            }

            // Fetch book issues from the repository
            var bookIssues = await _repository.GetByReaderIdAsync(readerId);

            if (bookIssues == null || !bookIssues.Any())
            {
                return NotFound(new { Message = "Не найдено записей о выдаче книг для указанного читателя." });
            }

            // Save result to cache if Redis caching is enabled
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
                await _cacheService.SetAsync(cacheKey, bookIssues, cacheTimeout);
            }

            return Ok(bookIssues);  // Return fresh data
        }

        // Method for creating a new book issue (no caching)
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] BookIssuesCreateDTO bookIssueCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdBookIssue = await _repository.CreateAsync(bookIssueCreateDTO);

            if (createdBookIssue == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Ошибка при создании записи о выдаче книги." });
            }

            return CreatedAtAction(nameof(Create), new { id = createdBookIssue.Id }, createdBookIssue);
        }

        // Method for deleting a book issue by id (no caching)
        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool deleted = await _repository.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new { Message = "Выдача книги с указанным id не найдена." });
            }

            return Ok(new { Message = "Выдача книги успешно удалена." });
        }

        // Method for updating a book issue (no caching)
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] BookIssuesUpdateDTO bookIssueUpdateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedBookIssue = await _repository.UpdateAsync(id, bookIssueUpdateDTO);

            if (updatedBookIssue == null)
            {
                return NotFound(new { Message = "Запись о выдаче книги с указанным id не найдена." });
            }

            return Ok(updatedBookIssue);
        }

        // Method for returning a book (no caching)
        [HttpPut("return/{id:int}")]
        public async Task<IActionResult> ReturnBook([FromRoute] int id, [FromBody] BookIssuesReturnDTO bookReturnDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var returnedBookIssue = await _repository.ReturnBookAsync(id, bookReturnDTO);

            if (returnedBookIssue == null)
            {
                return NotFound(new { Message = "Запись о выдаче книги с указанным id не найдена." });
            }

            return Ok(new { Message = "Книга успешно возвращена.", BookIssue = returnedBookIssue });
        }
    }
}
