using WEB.API.Context;
using Microsoft.EntityFrameworkCore;
using WEB.API.DTO;
using WEB.API.Models;
using WEB.API.Repository.Interfaces;

namespace WEB.API.Repository.Implementation
{
    public class PublishersRepository : IPublishersRepository
    {
        private readonly ApplicationDBContext _context;

        public PublishersRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<Publishers>> GetAllAsync()
        {
            return await _context.Publishers
                .ToListAsync();
        }

        public async Task<Publishers> GetAsync(int id)
        {
            return await _context.Publishers
                .FirstOrDefaultAsync(publisher => publisher.Id == id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Publishers publisher = await _context.Publishers.FirstOrDefaultAsync(publisher => publisher.Id == id);

            if (publisher == null)
            {
                return false;
            }

            _context.Publishers.Remove(publisher);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Publishers> UpdateAsync(int id, PublishersUpdateDTO DTO)
        {
            return null;
        }

        public async Task<Publishers> CreateAsync(Publishers publisher)
        {
            return null;
        }

    }
}
