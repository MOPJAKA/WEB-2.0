using WEB.API.DTO;
using WEB.API.Models;

namespace WEB.API.Repository.Interfaces
{
    public interface IBookIssuesRepository
    {
        public Task<List<BookIssues>> GetAllAsync();

        public Task<BookIssues> GetAsync(int id);

        public Task<bool> DeleteAsync(int id);

        public Task<BookIssues> UpdateAsync(int id, BookIssuesUpdateDTO DTO);

        public Task<BookIssues> CreateAsync(BookIssuesCreateDTO DTO);

        public Task<BookIssues> ReturnBookAsync(int id, BookIssuesReturnDTO bookReturnDTO);
        // update при возврате книги (дата возврата)

        public Task<List<BookIssues>> GetByReaderIdAsync(int? readerId);
        // ? - обозначает, что переменная readerId может быть равна null

        public Task<List<BookIssues>> GetOverdueIssuesAsync();
        // запрос на получение всех просроченных книг
    }
}
