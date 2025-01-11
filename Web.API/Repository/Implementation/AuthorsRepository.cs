using WEB.API.Context;
using Microsoft.EntityFrameworkCore;
using WEB.API.DTO;
using WEB.API.Models;
using WEB.API.Repository.Interfaces;

namespace WEB.API.Repository.Implementation
{
    public class AuthorsRepository : IAuthorsRepository
    {
        private readonly ApplicationDBContext _context;

        public AuthorsRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        // конструктор для получения связи с бд (_context - связь с бд)

        public async Task<List<Authors>> GetAllAsync()
        {
            return await _context.Authors
                .ToListAsync();
        } // выдаётся список авторов

        public async Task<Authors> GetAsync(int id)
        {
            return await _context.Authors
                .FirstOrDefaultAsync(author => author.Id == id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Authors author = await _context.Authors.FirstOrDefaultAsync(author => author.Id == id);

            if (author == null)
            {
                return false;
            }

            _context.Authors.Remove(author);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Authors> UpdateAsync(int id, AuthorsUpdateDTO DTO)
        {
            return null;
        }

        public async Task<Authors> CreateAsync(AuthorsCreateDTO DTO)
        {
            return null;
        }
    }
}
