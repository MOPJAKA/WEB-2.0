using WEB.API.Models;

namespace WEB.API.DTO
{
    public class BookIssuesUpdateDTO
    {
        public int BookId { get; set; }

        public int ReaderId { get; set; }

        public DateOnly IssueDate { get; set; }

        public DateOnly ExpectedReturnDate { get; set; }

        public DateOnly? ActualReturnDate { get; set; }
    }
}
