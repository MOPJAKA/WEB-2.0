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
                .Include(bi => bi.Book) // Жадная загрузка Book
                .Include(bi => bi.Reader) // Жадная загрузка Reader
                .ToListAsync();
        }

        public async Task<BookIssues> GetAsync(int id)
        {
            return await _context.BookIssues
                .Include(bi => bi.Book) // Жадная загрузка Book
                .Include(bi => bi.Reader) // Жадная загрузка Reader
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
            var bookIssue = await _context.BookIssues.FirstOrDefaultAsync(bi => bi.Id == id);
            // вытягиваем запись о выдаче книги из бд

            if (bookIssue == null)
            {
                return null;  // Если запись не найдена, возвращаем null
            }

            // Обновляем запись о выдаче книги
            bookIssue.BookId = DTO. BookId;
            bookIssue.ReaderId = DTO.ReaderId;
            bookIssue.IssueDate = DTO.IssueDate;
            bookIssue.ExpectedReturnDate = DTO.ExpectedReturnDate;
            bookIssue.ActualReturnDate = DTO.ActualReturnDate;

            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();

            return bookIssue;  // Возвращаем обновленную запись
        }

        public async Task<BookIssues> CreateAsync(BookIssuesCreateDTO DTO)
        {
            var bookIssue = new BookIssues
            {
                BookId = DTO.BookId,
                ReaderId = DTO.ReaderId,
                IssueDate = DTO.IssueDate,
                ExpectedReturnDate = DTO.ExpectedReturnDate,
                ActualReturnDate = DTO.ActualReturnDate
            };

            // Добавляем новую запись о выдаче книги
            await _context.BookIssues.AddAsync(bookIssue);
            await _context.SaveChangesAsync();

            return bookIssue;  // Возвращаем созданную запись
        }

        public async Task<BookIssues> ReturnBookAsync(int id, BookIssuesReturnDTO bookReturnDTO)
        {
            // Получаем запись о выдаче книги по Id
            var bookIssue = await _context.BookIssues.FirstOrDefaultAsync(bi => bi.Id == id);

            if (bookIssue == null)
            {
                return null;  // Если запись не найдена, возвращаем null
            }

            // Обновляем фактическую дату возврата
            bookIssue.ActualReturnDate = bookReturnDTO.ActualReturnDate;

            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();

            return bookIssue;  // Возвращаем обновленную запись
        }

        public async Task<List<BookIssues>> GetByReaderIdAsync(int? readerId)
        {
            var query = _context.BookIssues
                .Include(bi => bi.Book) // Жадная загрузка Book
                .Include(bi => bi.Reader) // Жадная загрузка Reader
                .AsQueryable()
                .Where(bi => bi.ActualReturnDate == null);

            if (readerId.HasValue)
            {
                query = query.Where(bi => bi.ReaderId == readerId.Value);                
            }

            return await query.ToListAsync();
        }

        public async Task<List<BookIssues>> GetOverdueIssuesAsync()
        {
            var overdueIssues = await _context.BookIssues
                .Include(bi => bi.Book) // Жадная загрузка Book
                .Include(bi => bi.Reader) // Жадная загрузка Reader
                .Where(bi => bi.ExpectedReturnDate < DateOnly.FromDateTime(DateTime.Now) && bi.ActualReturnDate == null)
                //возвращает только книги, которые были не возвращены (то есть, ActualReturnDate == null) и имеют истекший срок возврата (ExpectedReturnDate < DateOnly.FromDateTime(DateTime.Now)).
                //--Альтернатива--
                //(bi.ExpectedReturnDate < DateOnly.FromDateTime(DateTime.Now) && bi.ActualReturnDate == null) // Книги не возвращены и просрочены
                // || (bi.ActualReturnDate != null && bi.ActualReturnDate > bi.ExpectedReturnDate) // Книги возвращены позже срока
                .ToListAsync();

            return overdueIssues;
        }
    }
}
