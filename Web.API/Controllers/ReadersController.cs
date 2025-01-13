using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEB.API.DTO;
using WEB.API.Models;
using WEB.API.Repository.Implementation;
using WEB.API.Repository.Interfaces;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReadersController : ControllerBase
    {
        private readonly IReadersRepository _repository;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;

        public ReadersController(IReadersRepository repository, ICacheService cacheService, IConfiguration configuration)
        {
            _repository = repository;
            _cacheService = cacheService;
            _configuration = configuration;
        }

        // Method for getting all readers with caching
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cacheKey = CacheService.GenerateCacheKey(nameof(GetAll) + "Readers");

            // Check if Redis caching is enabled
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                // Attempt to get readers data from cache
                var cachedReaders = await _cacheService.GetAsync<IEnumerable<Readers>>(cacheKey);
                if (cachedReaders != null)
                {
                    return Ok(cachedReaders);  // Return cached data
                }
            }

            // If no data in cache, fetch from repository
            var readers = await _repository.GetAllAsync();

            // Save the readers data to the cache
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
                await _cacheService.SetAsync(cacheKey, readers, cacheTimeout);
            }

            return Ok(readers);  // Return the fresh data
        }

        // Method for getting a specific reader by id with caching
        [HttpGet("getreader/{id:int}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cacheKey = CacheService.GenerateCacheKey(nameof(Get) + "Readers", id);

            // Check if Redis caching is enabled
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                // Attempt to get reader data from cache
                var cachedReader = await _cacheService.GetAsync<Readers>(cacheKey);
                if (cachedReader != null)
                {
                    return Ok(cachedReader);  // Return cached data
                }
            }

            // If no data in cache, fetch from repository
            var reader = await _repository.GetAsync(id);
            if (reader == null)
            {
                return NotFound(new { Message = "Читатель с указанным id не найден." });
            }

            // Save the reader data to the cache
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
                await _cacheService.SetAsync(cacheKey, reader, cacheTimeout);
            }

            return Ok(reader);  // Return the fresh data
        }

        // Method for deleting a reader without caching
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
                return NotFound(new { Message = "Читатель с указанным id не найден." });
            }

            return Ok(new { Message = "Читатель успешно удален." });
        }

        // Method for creating a new reader without caching
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ReadersCreateDTO readerCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdReader = await _repository.CreateAsync(readerCreateDTO);

            if (createdReader == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Ошибка при создании читателя." });
            }

            return CreatedAtAction(nameof(Get), new { id = createdReader.Id }, createdReader); // Return the created reader with status 201
        }

        // Method for updating a reader without caching
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ReadersUpdateDTO readerUpdateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedReader = await _repository.UpdateAsync(id, readerUpdateDTO);

            if (updatedReader == null)
            {
                return NotFound(new { Message = "Читатель с указанным id не найден." });
            }

            return Ok(updatedReader); // Return the updated reader
        }
    }
}
