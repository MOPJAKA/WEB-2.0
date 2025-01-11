using WEB.API.DTO;
using WEB.API.Models;

namespace WEB.API.Repository.Interfaces
{
    public interface IBookIssuesRepository
    {
        public Task<List<BookIssues>> GetAllAsync();

        public Task<BookIssues> GetAsync(int id);

        public Task<bool> DeleteAsync(int id);

        public Task<BookIssues> UpdateAsync(int id, BookIssuesUpdateDTO DTO);

        public Task<BookIssues> CreateAsync(int id, BookIssuesCreateDTO DTO);
    }
}
