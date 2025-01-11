using WEB.API.DTO;
using WEB.API.Models;

namespace WEB.API.Repository.Interfaces
{
    public interface IAuthorsRepository
    {
        public Task<List<Authors>> GetAllAsync(); 
        // вывод всех данных без where

        public Task<Authors> GetAsync(int id);
        // вывод всех данных c where (при определённом id)

        public Task<bool> DeleteAsync(int id);

        public Task<Authors> UpdateAsync(int id, AuthorsUpdateDTO DTO);

        public Task<Authors> CreateAsync(AuthorsCreateDTO DTO);
    }
}
