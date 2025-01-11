using System;

namespace WEB.API.Models
{
    public class BookIssues
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        public int ReaderId { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime ExpectedReturnDate { get; set; }

        public DateTime ActualReturnDate { get; set; }

        public Books Book { get; set; }
        // у одной книги мб много выдачи
        // это множество
        public Readers Reader { get; set; }
        // у одного читателя мб много выдачи
    }
}
