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
    public class BooksController : ControllerBase
    {
        private readonly IBooksRepository _repository;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;

        public BooksController(IBooksRepository repository, ICacheService cacheService, IConfiguration configuration)
        {
            _repository = repository;
            _cacheService = cacheService;
            _configuration = configuration;
        }

        // Method for getting all books with caching
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cacheKey = CacheService.GenerateCacheKey(nameof(GetAll) + "Book");

            // Check if Redis caching is enabled
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                // Attempt to get books data from cache
                var cachedBooks = await _cacheService.GetAsync<IEnumerable<Books>>(cacheKey);
                if (cachedBooks != null)
                {
                    return Ok(cachedBooks);  // Return cached data
                }
            }

            // If no data in cache, fetch from repository
            var books = await _repository.GetAllAsync();

            // Save the books data to the cache
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
                await _cacheService.SetAsync(cacheKey, books, cacheTimeout);
            }

            return Ok(books);  // Return the fresh data
        }

        // Method for getting a specific book by id with caching
        [HttpGet("getbook/{id:int}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cacheKey = CacheService.GenerateCacheKey(nameof(Get) + "Book", id);

            // Check if Redis caching is enabled
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                // Attempt to get book data from cache
                var cachedBook = await _cacheService.GetAsync<Books>(cacheKey);
                if (cachedBook != null)
                {
                    return Ok(cachedBook);  // Return cached data
                }
            }

            // If no data in cache, fetch from repository
            var book = await _repository.GetAsync(id);
            if (book == null)
            {
                return NotFound(new { Message = "Книга с указанным id не найдена." });
            }

            // Save the book data to the cache
            if (_configuration.GetValue<bool>("Redis:Enabled"))
            {
                var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
                await _cacheService.SetAsync(cacheKey, book, cacheTimeout);
            }

            return Ok(book);  // Return the fresh data
        }

        // Method for deleting a book without caching
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
                return NotFound(new { Message = "Книга с указанным id не найдена." });
            }

            return Ok(new { Message = "Книга успешно удалена." });
        }

        // Method for creating a new book without caching
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] BooksCreateDTO booksCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdBook = await _repository.CreateAsync(booksCreateDTO);

            if (createdBook == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Ошибка при создании книги." });
            }

            return CreatedAtAction(nameof(Get), new { id = createdBook.Id }, createdBook); // Return the created book with status 201
        }

        // Method for updating a book without caching
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] BooksUpdateDTO booksUpdateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedBook = await _repository.UpdateAsync(id, booksUpdateDTO);

            if (updatedBook == null)
            {
                return NotFound(new { Message = "Книга с указанным id не найдена." });
            }

            return Ok(updatedBook); // Return the updated book
        }
    }
}
