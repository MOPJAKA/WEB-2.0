using WEB.API.Context;
using Microsoft.EntityFrameworkCore;
using WEB.API.DTO;
using WEB.API.Models;
using WEB.API.Repository.Interfaces;

namespace WEB.API.Repository.Implementation
{
    public class BookIssuesRepository : IBookIssuesRepository
    {
        private readonly ApplicationDBContext _context;

        public BookIssuesRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<BookIssues>> GetAllAsync()
        {
            return await _context.BookIssues
                .ToListAsync();
        }

        public async Task<BookIssues> GetAsync(int id)
        {
            return await _context.BookIssues
                .FirstOrDefaultAsync(BI => BI.Id == id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            BookIssues BookIssue = await _context.BookIssues.FirstOrDefaultAsync(BookIssue => BookIssue.Id == id);

            if (BookIssue == null)
            {
                return false;
            }

            _context.BookIssues.Remove(BookIssue);  

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<BookIssues> UpdateAsync(int id, BookIssuesUpdateDTO DTO)
        {
            return null;
        }

        public async Task<BookIssues> CreateAsync(int id, BookIssuesCreateDTO DTO)
        {
            return null;
        }
    }
}
