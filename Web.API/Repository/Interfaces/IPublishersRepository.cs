using WEB.API.DTO;
using WEB.API.Models;

namespace WEB.API.Repository.Interfaces
{
    public interface IPublishersRepository
    {

        public Task<List<Publishers>> GetAllAsync();

        public Task<Publishers> GetAsync(int id);

        public Task<bool> DeleteAsync(int id);

        public Task<Publishers> UpdateAsync(int id, PublishersUpdateDTO DTO);

        public Task<Publishers> CreateAsync(Publishers publisher);
    }
}
