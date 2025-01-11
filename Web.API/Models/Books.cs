using System;

namespace WEB.API.Models
{
    public class Books
    {
        public int Id { get; set; }

        public int AuthorId { get; set; }

        public int PublisherId { get; set; }

        public string Title { get; set; }

        public uint PublisherYear { get; set; }

        public string LibraryLocation { get; set; }       

        public Authors Author { get; set; }
        // у одного автора много книг
        public Publishers Publisher { get; set; }
        // у одного издательства много книг
    }
}
