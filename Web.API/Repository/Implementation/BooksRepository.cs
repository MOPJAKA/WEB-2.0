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
                .Include(bi => bi.Author) // Жадная загрузка Book
                .Include(bi => bi.Publisher) // Жадная загрузка Reader
                .ToListAsync();
        }

        public async Task<Books> GetAsync(int id)
        {
            return await _context.Books
                .Include(bi => bi.Author) // Жадная загрузка Book
                .Include(bi => bi.Publisher) // Жадная загрузка Reader
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

        public async Task<Books> CreateAsync(BooksCreateDTO DTO)
        {
            // Создание нового объекта Books из DTO
            var book = new Books
            {
                AuthorId = DTO.AuthorId,
                PublisherId = DTO.PublisherId,
                Title = DTO.Title,
                PublisherYear = DTO.PublisherYear,
                LibraryLocation = DTO.LibraryLocation
            };

            // Добавление книги в контекст
            await _context.Books.AddAsync(book);
            // Сохранение изменений в базе данных
            await _context.SaveChangesAsync();

            return book; // Возвращаем созданную книгу
        }


        public async Task<Books> UpdateAsync(int id, BooksUpdateDTO DTO)
        {
            // Находим книгу по id
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return null; // Книга не найдена, возвращаем null
            }

            // Обновляем данные книги
            book.AuthorId = DTO.AuthorId;
            book.PublisherId = DTO.PublisherId;
            book.Title = DTO.Title;
            book.PublisherYear = DTO.PublisherYear;
            book.LibraryLocation = DTO.LibraryLocation;

            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();

            return book; // Возвращаем обновленную книгу
        }

    }
}
