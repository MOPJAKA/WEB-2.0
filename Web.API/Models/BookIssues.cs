using System;

namespace WEB.API.Models
{
    public class BookIssues
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        public int ReaderId { get; set; }

        public DateOnly IssueDate { get; set; }

        public DateOnly ExpectedReturnDate { get; set; }

        public DateOnly? ActualReturnDate { get; set; }  // Nullable DateOnly

        public Books Book { get; set; }
        // У одной книги может быть много выдач (множество)
        public Readers Reader { get; set; }
        // У одного читателя может быть много выдач (множество)
    }
}
