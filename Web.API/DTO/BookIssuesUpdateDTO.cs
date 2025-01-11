using WEB.API.Models;

namespace WEB.API.DTO
{
    public class BookIssuesUpdateDTO
    {
        public int BookId { get; set; }

        public int ReaderId { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime ExpectedReturnDate { get; set; }

        public DateTime ActualReturnDate { get; set; }
    }
}
