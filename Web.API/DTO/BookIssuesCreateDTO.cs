namespace WEB.API.DTO
{
    public class BookIssuesCreateDTO
    {
        public int BookId { get; set; }

        public int ReaderId { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime ExpectedReturnDate { get; set; }

        public DateTime ActualReturnDate { get; set; }
    }
}
