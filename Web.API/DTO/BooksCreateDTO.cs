namespace WEB.API.DTO
{
    public class BooksCreateDTO
    {
        public int AuthorId { get; set; }

        public int PublisherId { get; set; }

        public string Title { get; set; }

        public uint PublisherYear { get; set; }

        public string LibraryLocation { get; set; }
    }
}
