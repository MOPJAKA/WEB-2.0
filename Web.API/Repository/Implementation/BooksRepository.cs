using WEB.API.Context;
using Microsoft.EntityFrameworkCore;
using WEB.API.DTO;
using WEB.API.Models;
using WEB.API.Repository.Interfaces;

namespace WEB.API.Repository.Implementation
{
    public class BooksRepository : IBooksRepository
    {
        private readonly ApplicationDBContext _context;

        public BooksRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<Books>> GetAllAsync()
        {
            return await _context.Books
                .ToListAsync();
        }

        public async Task<Books> GetAsync(int id)
        {
            return await _context.Books
                .FirstOrDefaultAsync(book => book.Id == id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Books book = await _context.Books.FirstOrDefaultAsync(book => book.Id == id);

            if (book == null)
            {
                return false; 
            }

            _context.Books.Remove(book);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Books> UpdateAsync(int id, BooksUpdateDTO DTO)
        {
            return null;
        }

        public async Task<Books> CreateAsync(BooksCreateDTO DTO)
        {
            return null;
        }
    }
}
