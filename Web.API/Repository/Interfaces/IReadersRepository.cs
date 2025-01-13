using WEB.API.DTO;
using WEB.API.Models;

namespace WEB.API.Repository.Interfaces
{
    public interface IReadersRepository
    {
        public Task<List<Readers>> GetAllAsync();

        public Task<Readers> GetAsync(int id);

        public Task<bool> DeleteAsync(int id);

        public Task<Readers> UpdateAsync(int id, ReadersUpdateDTO DTO);

        public Task<Readers> CreateAsync(ReadersCreateDTO DTO);
    }
}
