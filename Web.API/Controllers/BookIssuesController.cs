using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEB.API.Repository.Interfaces;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookIssuesController : ControllerBase
    {
        private readonly IBookIssuesRepository _repository;

        public BookIssuesController(IBookIssuesRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await _repository.GetAllAsync());
        }

        [HttpGet("getbookissue/{id:int}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await _repository.GetAsync(id));
        }

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
    }
}
