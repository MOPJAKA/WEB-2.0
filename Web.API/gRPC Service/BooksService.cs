using Grpc.Core;
using WEB.API.Models;
using WEB.API.Repository.Interfaces;
using WEB.API.Protos.Books;
using WEB.API.DTO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WEB.API.Repository.Implementation;

public class BooksServiceImpl : BooksService.BooksServiceBase
{
    private readonly IBooksRepository _bookRepository;
    private readonly ICacheService _cacheService; // Inject cache service
    private readonly IConfiguration _configuration; // Inject configuration

    public BooksServiceImpl(IBooksRepository bookRepository, ICacheService cacheService, IConfiguration configuration)
    {
        _bookRepository = bookRepository;
        _cacheService = cacheService;
        _configuration = configuration;
    }

    // Method to get all books with caching
    public override async Task<GetAllBooksResponse> GetAllBooks(BooksEmptyRequest request, ServerCallContext context)
    {
        var cacheKey = CacheService.GenerateCacheKey(nameof(GetAllBooks));

        // Check if caching is enabled and if data is in cache
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cachedBooks = await _cacheService.GetAsync<IEnumerable<Book>>(cacheKey);
            if (cachedBooks != null)
            {
                return new GetAllBooksResponse { Books = { cachedBooks } };
            }
        }

        // If not in cache, retrieve from repository
        var books = await _bookRepository.GetAllAsync();
        var response = new GetAllBooksResponse();
        response.Books.AddRange(books.Select(b => new Book
        {
            Id = b.Id,
            Title = b.Title,
            AuthorId = b.AuthorId,
            PublisherId = b.PublisherId,
            PublisherYear = b.PublisherYear,
            LibraryLocation = b.LibraryLocation
        }));

        // Store in cache if enabled
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
            await _cacheService.SetAsync(cacheKey, response.Books, cacheTimeout);
        }

        return response;
    }

    // Method to get a book by ID with caching
    public override async Task<GetBookResponse> GetBook(GetBookRequest request, ServerCallContext context)
    {
        var cacheKey = CacheService.GenerateCacheKey(nameof(GetBook), request.Id);

        // Check if caching is enabled and if data is in cache
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cachedBook = await _cacheService.GetAsync<Book>(cacheKey);
            if (cachedBook != null)
            {
                return new GetBookResponse
                {
                    Book = cachedBook
                };
            }
        }

        // If not in cache, retrieve from repository
        var book = await _bookRepository.GetAsync(request.Id);
        if (book == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Book not found"));
        }

        // Store in cache if enabled
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
            await _cacheService.SetAsync(cacheKey, new Book
            {
                Id = book.Id,
                Title = book.Title,
                AuthorId = book.AuthorId,
                PublisherId = book.PublisherId,
                PublisherYear = book.PublisherYear,
                LibraryLocation = book.LibraryLocation
            }, cacheTimeout);
        }

        return new GetBookResponse
        {
            Book = new Book
            {
                Id = book.Id,
                Title = book.Title,
                AuthorId = book.AuthorId,
                PublisherId = book.PublisherId,
                PublisherYear = book.PublisherYear,
                LibraryLocation = book.LibraryLocation
            }
        };
    }

    // Method to create a book with cache invalidation
    public override async Task<CreateBookResponse> CreateBook(CreateBookRequest request, ServerCallContext context)
    {
        var bookDto = new BooksCreateDTO
        {
            Title = request.Title,
            AuthorId = request.AuthorId,
            PublisherId = request.PublisherId,
            PublisherYear = request.PublisherYear,
            LibraryLocation = request.LibraryLocation
        };

        var createdBook = await _bookRepository.CreateAsync(bookDto);

        // Cache invalidation after creation (optional, depending on use case)
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cacheKey = CacheService.GenerateCacheKey(nameof(GetAllBooks)); // Invalidating all books list cache
            await _cacheService.RemoveAsync(cacheKey);
        }

        return new CreateBookResponse
        {
            Book = new Book
            {
                Id = createdBook.Id,
                Title = createdBook.Title,
                AuthorId = createdBook.AuthorId,
                PublisherId = createdBook.PublisherId,
                PublisherYear = createdBook.PublisherYear,
                LibraryLocation = createdBook.LibraryLocation
            }
        };
    }

    // Method to update a book with cache invalidation
    public override async Task<UpdateBookResponse> UpdateBook(UpdateBookRequest request, ServerCallContext context)
    {
        var bookDto = new BooksUpdateDTO
        {
            Title = request.Title,
            AuthorId = request.AuthorId,
            PublisherId = request.PublisherId,
            PublisherYear = request.PublisherYear,
            LibraryLocation = request.LibraryLocation
        };

        var updatedBook = await _bookRepository.UpdateAsync(request.Id, bookDto);
        if (updatedBook == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Book not found"));
        }

        return new UpdateBookResponse
        {
            Book = new Book
            {
                Id = updatedBook.Id,
                Title = updatedBook.Title,
                AuthorId = updatedBook.AuthorId,
                PublisherId = updatedBook.PublisherId,
                PublisherYear = updatedBook.PublisherYear,
                LibraryLocation = updatedBook.LibraryLocation
            }
        };
    }

    // Method to delete a book with cache invalidation
    public override async Task<DeleteBookResponse> DeleteBook(DeleteBookRequest request, ServerCallContext context)
    {
        var success = await _bookRepository.DeleteAsync(request.Id);
        if (!success)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Book not found"));
        }

        return new DeleteBookResponse { Success = true };
    }
}
