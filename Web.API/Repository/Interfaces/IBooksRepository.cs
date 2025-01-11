using WEB.API.DTO;
using WEB.API.Models;

namespace WEB.API.Repository.Interfaces
{
    public interface IBooksRepository
    {
        public Task<List<Books>> GetAllAsync();

        public Task<Books> GetAsync(int id);

        public Task<bool> DeleteAsync(int id);

        public Task<Books> UpdateAsync(int id, BooksUpdateDTO DTO);

        public Task<Books> CreateAsync(BooksCreateDTO DTO);
    }
}
