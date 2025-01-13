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
    public class PublishersController : ControllerBase
    {
        private readonly IPublishersRepository _repository;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;

        public PublishersController(IPublishersRepository repository, ICacheService cacheService, IConfiguration configuration)
        {
            _repository = repository;
            _cacheService = cacheService;
            _configuration = configuration;
        }

        // Method for getting all publishers with caching
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cacheKey = CacheService.GenerateCacheKey(nameof(GetAll) + "Publishers");

            // Check if Redis caching is enabled
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                // Attempt to get publishers data from cache
                var cachedPublishers = await _cacheService.GetAsync<IEnumerable<Publishers>>(cacheKey);
                if (cachedPublishers != null)
                {
                    return Ok(cachedPublishers);  // Return cached data
                }
            }

            // If no data in cache, fetch from repository
            var publishers = await _repository.GetAllAsync();

            // Save the publishers data to the cache
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
                await _cacheService.SetAsync(cacheKey, publishers, cacheTimeout);
            }

            return Ok(publishers);  // Return the fresh data
        }

        // Method for getting a specific publisher by id with caching
        [HttpGet("getpublisher/{id:int}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cacheKey = CacheService.GenerateCacheKey(nameof(Get) + "Publishers", id);

            // Check if Redis caching is enabled
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                // Attempt to get publisher data from cache
                var cachedPublisher = await _cacheService.GetAsync<Publishers>(cacheKey);
                if (cachedPublisher != null)
                {
                    return Ok(cachedPublisher);  // Return cached data
                }
            }

            // If no data in cache, fetch from repository
            var publisher = await _repository.GetAsync(id);
            if (publisher == null)
            {
                return NotFound(new { Message = "Издатель с указанным id не найден." });
            }

            // Save the publisher data to the cache
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
                await _cacheService.SetAsync(cacheKey, publisher, cacheTimeout);
            }

            return Ok(publisher);  // Return the fresh data
        }

        // Method for deleting a publisher without caching
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
                return NotFound(new { Message = "Издатель с указанным id не найден." });
            }

            return Ok(new { Message = "Издатель успешно удален." });
        }

        // Method for creating a new publisher without caching
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] PublishersCreateDTO publisherCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdPublisher = await _repository.CreateAsync(publisherCreateDTO);

            if (createdPublisher == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Ошибка при создании издателя." });
            }

            return CreatedAtAction(nameof(Get), new { id = createdPublisher.Id }, createdPublisher); // Return the created publisher with status 201
        }

        // Method for updating a publisher without caching
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PublishersUpdateDTO publisherUpdateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedPublisher = await _repository.UpdateAsync(id, publisherUpdateDTO);

            if (updatedPublisher == null)
            {
                return NotFound(new { Message = "Издатель с указанным id не найден." });
            }

            return Ok(updatedPublisher); // Return the updated publisher
        }
    }
}
