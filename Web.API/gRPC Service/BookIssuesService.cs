using Grpc.Core;
using WEB.API.Models;
using WEB.API.Repository.Interfaces;
using WEB.API.Protos.BookIssues;
using WEB.API.DTO;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WEB.API.Repository.Implementation;

public class BookIssuesServiceImpl : BookIssuesService.BookIssuesServiceBase
{
    private readonly IBookIssuesRepository _bookIssuesRepository;
    private readonly ICacheService _cacheService;
    private readonly IConfiguration _configuration;

    public BookIssuesServiceImpl(IBookIssuesRepository bookIssuesRepository, ICacheService cacheService, IConfiguration configuration)
    {
        _bookIssuesRepository = bookIssuesRepository;
        _cacheService = cacheService;
        _configuration = configuration;
    }

    public override async Task<GetBookIssueResponse> GetBookIssue(GetBookIssueRequest request, ServerCallContext context)
    {
        var cacheKey = CacheService.GenerateCacheKey(nameof(GetBookIssue) + "BookIssues");

        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cachedBookIssue = await _cacheService.GetAsync<BookIssue>(cacheKey);
            if (cachedBookIssue != null)
            {
                return new GetBookIssueResponse { BookIssue = cachedBookIssue };
            }
        }

        var bookIssue = await _bookIssuesRepository.GetAsync(request.Id);
        if (bookIssue == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Book issue not found"));
        }

        var bookIssueResponse = new BookIssue
        {
            Id = bookIssue.Id,
            BookId = bookIssue.BookId,
            ReaderId = bookIssue.ReaderId,
            IssueDate = bookIssue.IssueDate.ToString("yyyy-MM-dd"),
            ExpectedReturnDate = bookIssue.ExpectedReturnDate.ToString("yyyy-MM-dd"),
            ActualReturnDate = bookIssue.ActualReturnDate?.ToString("yyyy-MM-dd") ?? string.Empty
        };

        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
            await _cacheService.SetAsync(cacheKey, bookIssueResponse, cacheTimeout);
        }

        return new GetBookIssueResponse { BookIssue = bookIssueResponse };
    }

    public override async Task<GetAllBookIssuesResponse> GetAllBookIssues(BookIssuesEmptyRequest request, ServerCallContext context)
    {
        var cacheKey = CacheService.GenerateCacheKey(nameof(GetAllBookIssues) + "BookIssues");

        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cachedBookIssues = await _cacheService.GetAsync<IEnumerable<BookIssue>>(cacheKey);
            if (cachedBookIssues != null)
            {
                var response = new GetAllBookIssuesResponse();
                response.BookIssues.AddRange(cachedBookIssues);
                return response;
            }
        }

        var bookIssues = await _bookIssuesRepository.GetAllAsync();

        var bookIssuesResponse = bookIssues.Select(bookIssue => new BookIssue
        {
            Id = bookIssue.Id,
            BookId = bookIssue.BookId,
            ReaderId = bookIssue.ReaderId,
            IssueDate = bookIssue.IssueDate.ToString("yyyy-MM-dd"),
            ExpectedReturnDate = bookIssue.ExpectedReturnDate.ToString("yyyy-MM-dd"),
            ActualReturnDate = bookIssue.ActualReturnDate?.ToString("yyyy-MM-dd") ?? string.Empty
        }).ToList();

        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
            await _cacheService.SetAsync(cacheKey, bookIssuesResponse, cacheTimeout);
        }

        var responseAll = new GetAllBookIssuesResponse();
        responseAll.BookIssues.AddRange(bookIssuesResponse);
        return responseAll;
    }

    public override async Task<CreateBookIssueResponse> CreateBookIssue(CreateBookIssueRequest request, ServerCallContext context)
    {
        DateOnly issueDate = DateOnly.Parse(request.IssueDate);
        DateOnly expectedReturnDate = DateOnly.Parse(request.ExpectedReturnDate);
        DateOnly? actualReturnDate = string.IsNullOrEmpty(request.ActualReturnDate)
            ? (DateOnly?)null
            : DateOnly.Parse(request.ActualReturnDate);

        var bookIssueDto = new BookIssuesCreateDTO
        {
            BookId = request.BookId,
            ReaderId = request.ReaderId,
            IssueDate = issueDate,
            ExpectedReturnDate = expectedReturnDate,
            ActualReturnDate = actualReturnDate
        };

        var createdBookIssue = await _bookIssuesRepository.CreateAsync(bookIssueDto);

        return new CreateBookIssueResponse
        {
            BookIssue = new BookIssue
            {
                Id = createdBookIssue.Id,
                BookId = createdBookIssue.BookId,
                ReaderId = createdBookIssue.ReaderId,
                IssueDate = createdBookIssue.IssueDate.ToString("yyyy-MM-dd"),
                ExpectedReturnDate = createdBookIssue.ExpectedReturnDate.ToString("yyyy-MM-dd"),
                ActualReturnDate = createdBookIssue.ActualReturnDate?.ToString("yyyy-MM-dd") ?? string.Empty
            }
        };
    }

    public override async Task<UpdateBookIssueResponse> UpdateBookIssue(UpdateBookIssueRequest request, ServerCallContext context)
    {
        DateOnly issueDate = DateOnly.Parse(request.IssueDate);
        DateOnly expectedReturnDate = DateOnly.Parse(request.ExpectedReturnDate);
        DateOnly? actualReturnDate = string.IsNullOrEmpty(request.ActualReturnDate)
            ? (DateOnly?)null
            : DateOnly.Parse(request.ActualReturnDate);

        var bookIssueDto = new BookIssuesUpdateDTO
        {
            BookId = request.BookId,
            ReaderId = request.ReaderId,
            IssueDate = issueDate,
            ExpectedReturnDate = expectedReturnDate,
            ActualReturnDate = actualReturnDate
        };

        var updatedBookIssue = await _bookIssuesRepository.UpdateAsync(request.Id, bookIssueDto);
        if (updatedBookIssue == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Book issue not found"));
        }

        var cacheKey = CacheService.GenerateCacheKey(nameof(UpdateBookIssue) + "BookIssues");
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
            await _cacheService.SetAsync(cacheKey, updatedBookIssue, cacheTimeout);
        }

        return new UpdateBookIssueResponse
        {
            BookIssue = new BookIssue
            {
                Id = updatedBookIssue.Id,
                BookId = updatedBookIssue.BookId,
                ReaderId = updatedBookIssue.ReaderId,
                IssueDate = updatedBookIssue.IssueDate.ToString("yyyy-MM-dd"),
                ExpectedReturnDate = updatedBookIssue.ExpectedReturnDate.ToString("yyyy-MM-dd"),
                ActualReturnDate = updatedBookIssue.ActualReturnDate?.ToString("yyyy-MM-dd") ?? string.Empty
            }
        };
    }

    public override async Task<DeleteBookIssueResponse> DeleteBookIssue(DeleteBookIssueRequest request, ServerCallContext context)
    {
        var success = await _bookIssuesRepository.DeleteAsync(request.Id);
        if (!success)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Book issue not found"));
        }

        var cacheKey = CacheService.GenerateCacheKey(nameof(DeleteBookIssue) + "BookIssues");
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            await _cacheService.RemoveAsync(cacheKey);
        }

        return new DeleteBookIssueResponse { Success = true };
    }

    public override async Task<GetOverdueBookIssuesResponse> GetOverdueBookIssues(BookIssuesEmptyRequest request, ServerCallContext context)
    {
        var cacheKey = CacheService.GenerateCacheKey(nameof(GetOverdueBookIssues) + "BookIssues");

        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cachedOverdueIssues = await _cacheService.GetAsync<IEnumerable<BookIssue>>(cacheKey);
            if (cachedOverdueIssues != null)
            {
                var response = new GetOverdueBookIssuesResponse();
                response.BookIssues.AddRange(cachedOverdueIssues);
                return response;
            }
        }

        var overdueBookIssues = await _bookIssuesRepository.GetOverdueIssuesAsync();

        var overdueIssuesResponse = overdueBookIssues.Select(bookIssue => new BookIssue
        {
            Id = bookIssue.Id,
            BookId = bookIssue.BookId,
            ReaderId = bookIssue.ReaderId,
            IssueDate = bookIssue.IssueDate.ToString("yyyy-MM-dd"),
            ExpectedReturnDate = bookIssue.ExpectedReturnDate.ToString("yyyy-MM-dd"),
            ActualReturnDate = bookIssue.ActualReturnDate?.ToString("yyyy-MM-dd") ?? string.Empty
        }).ToList();

        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
            await _cacheService.SetAsync(cacheKey, overdueIssuesResponse, cacheTimeout);
        }

        var responseOverdue = new GetOverdueBookIssuesResponse();
        responseOverdue.BookIssues.AddRange(overdueIssuesResponse);
        return responseOverdue;
    }

    public override async Task<GetBookIssuesByReaderResponse> GetBookIssuesByReader(GetBookIssuesByReaderRequest request, ServerCallContext context)
    {
        var cacheKey = CacheService.GenerateCacheKey(nameof(GetBookIssuesByReader) + "BookIssues");

        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cachedIssuesByReader = await _cacheService.GetAsync<IEnumerable<BookIssue>>(cacheKey);
            if (cachedIssuesByReader != null)
            {
                var response = new GetBookIssuesByReaderResponse();
                response.BookIssues.AddRange(cachedIssuesByReader);
                return response;
            }
        }

        var bookIssuesByReader = await _bookIssuesRepository.GetByReaderIdAsync(request.ReaderId);

        var issuesByReaderResponse = bookIssuesByReader.Select(bookIssue => new BookIssue
        {
            Id = bookIssue.Id,
            BookId = bookIssue.BookId,
            ReaderId = bookIssue.ReaderId,
            IssueDate = bookIssue.IssueDate.ToString("yyyy-MM-dd"),
            ExpectedReturnDate = bookIssue.ExpectedReturnDate.ToString("yyyy-MM-dd"),
            ActualReturnDate = bookIssue.ActualReturnDate?.ToString("yyyy-MM-dd") ?? string.Empty
        }).ToList();

        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
            await _cacheService.SetAsync(cacheKey, issuesByReaderResponse, cacheTimeout);
        }

        var responseByReader = new GetBookIssuesByReaderResponse();
        responseByReader.BookIssues.AddRange(issuesByReaderResponse);
        return responseByReader;
    }

    public override async Task<ReturnBookIssuesResponse> ReturnBookIssues(ReturnBookIssuesRequest request, ServerCallContext context)
    {
        Console.WriteLine($"Received ReturnBookIssues request with ID {request.Id} and ActualReturnDate {request.ActualReturnDate}");

        DateOnly actualReturnDate;
        try
        {
            actualReturnDate = DateOnly.ParseExact(request.ActualReturnDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid date format. Use 'yyyy-MM-dd'."));
        }

        var bookReturnDto = new BookIssuesReturnDTO
        {
            ActualReturnDate = actualReturnDate
        };

        var returnedBookIssue = await _bookIssuesRepository.ReturnBookAsync(request.Id, bookReturnDto);

        if (returnedBookIssue == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Book issue not found"));
        }

        return new ReturnBookIssuesResponse
        {
            BookIssue = new BookIssue
            {
                Id = returnedBookIssue.Id,
                BookId = returnedBookIssue.BookId,
                ReaderId = returnedBookIssue.ReaderId,
                IssueDate = returnedBookIssue.IssueDate.ToString("yyyy-MM-dd"),
                ExpectedReturnDate = returnedBookIssue.ExpectedReturnDate.ToString("yyyy-MM-dd"),
                ActualReturnDate = returnedBookIssue.ActualReturnDate?.ToString("yyyy-MM-dd") ?? string.Empty
            }
        };
    }
}
