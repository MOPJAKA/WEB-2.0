using WEB.API.Context;
using Microsoft.EntityFrameworkCore;
using WEB.API.DTO;
using WEB.API.Models;
using WEB.API.Repository.Interfaces;

namespace WEB.API.Repository.Implementation
{
    public class ReadersRepository : IReadersRepository
    {
        private readonly ApplicationDBContext _context;

        public ReadersRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<Readers>> GetAllAsync()
        {
            return await _context.Readers
                .ToListAsync();
        }

        public async Task<Readers> GetAsync(int id)
        {
            return await _context.Readers
                .FirstOrDefaultAsync(reader => reader.Id == id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Readers reader = await _context.Readers.FirstOrDefaultAsync(reader => reader.Id == id);

            if (reader == null)
            {
                return false;
            }

            _context.Readers.Remove(reader);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Readers> UpdateAsync(int id, ReadersUpdateDTO DTO)
        {
            return null;
        }

        public async Task<Readers> CreateAsync(int id, ReadersCreateDTO DTO)
        {
            return null;
        }

    }
}
