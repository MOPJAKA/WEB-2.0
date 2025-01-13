using Grpc.Core;
using WEB.API.Models;
using WEB.API.DTO;
using WEB.API.Repository.Interfaces;
using WEB.API.Protos.Authors;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WEB.API.Repository.Implementation;

public class AuthorsServiceImpl : AuthorsService.AuthorsServiceBase
{
    private readonly IAuthorsRepository _authorsRepository;
    private readonly ICacheService _cacheService; // Inject cache service
    private readonly IConfiguration _configuration; // Inject configuration

    public AuthorsServiceImpl(IAuthorsRepository authorsRepository, ICacheService cacheService, IConfiguration configuration)
    {
        _authorsRepository = authorsRepository;
        _cacheService = cacheService;
        _configuration = configuration;
    }

    // Method to get all authors with caching
    public override async Task<GetAllAuthorsResponse> GetAllAuthors(AuthorsEmptyRequest request, ServerCallContext context)
    {
        var cacheKey = CacheService.GenerateCacheKey(nameof(GetAllAuthors));

        // Check if caching is enabled and if data is in cache
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cachedAuthors = await _cacheService.GetAsync<IEnumerable<Author>>(cacheKey);
            if (cachedAuthors != null)
            {
                return new GetAllAuthorsResponse { Authors = { cachedAuthors } };
            }
        }

        // If not in cache, retrieve from repository
        var authors = await _authorsRepository.GetAllAsync();
        var response = new GetAllAuthorsResponse();
        response.Authors.AddRange(authors.Select(a => new Author
        {
            Id = a.Id,
            FirstName = a.FirstName,
            LastName = a.LastName
        }));

        // Store in cache if enabled
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
            await _cacheService.SetAsync(cacheKey, response.Authors, cacheTimeout);
        }

        return response;
    }

    // Method to get an author by ID with caching
    public override async Task<GetAuthorResponse> GetAuthor(GetAuthorRequest request, ServerCallContext context)
    {
        var cacheKey = CacheService.GenerateCacheKey(nameof(GetAuthor), request.Id);

        // Check if caching is enabled and if data is in cache
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cachedAuthor = await _cacheService.GetAsync<Author>(cacheKey);
            if (cachedAuthor != null)
            {
                return new GetAuthorResponse
                {
                    Author = cachedAuthor
                };
            }
        }

        // If not in cache, retrieve from repository
        var author = await _authorsRepository.GetAsync(request.Id);
        if (author == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Author not found"));
        }

        // Store in cache if enabled
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
            await _cacheService.SetAsync(cacheKey, new Author
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName
            }, cacheTimeout);
        }

        return new GetAuthorResponse
        {
            Author = new Author
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName
            }
        };
    }

    // Method to create an author without caching
    public override async Task<CreateAuthorResponse> CreateAuthor(CreateAuthorRequest request, ServerCallContext context)
    {
        var authorDto = new AuthorsCreateDTO
        {
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var createdAuthor = await _authorsRepository.CreateAsync(authorDto);

        return new CreateAuthorResponse
        {
            Author = new Author
            {
                Id = createdAuthor.Id,
                FirstName = createdAuthor.FirstName,
                LastName = createdAuthor.LastName
            }
        };
    }

    // Method to update an author without caching
    public override async Task<UpdateAuthorResponse> UpdateAuthor(UpdateAuthorRequest request, ServerCallContext context)
    {
        var authorDto = new AuthorsUpdateDTO
        {
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var updatedAuthor = await _authorsRepository.UpdateAsync(request.Id, authorDto);
        if (updatedAuthor == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Author not found"));
        }

        return new UpdateAuthorResponse
        {
            Author = new Author
            {
                Id = updatedAuthor.Id,
                FirstName = updatedAuthor.FirstName,
                LastName = updatedAuthor.LastName
            }
        };
    }

    // Method to delete an author without caching
    public override async Task<DeleteAuthorResponse> DeleteAuthor(DeleteAuthorRequest request, ServerCallContext context)
    {
        var success = await _authorsRepository.DeleteAsync(request.Id);
        if (!success)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Author not found"));
        }

        return new DeleteAuthorResponse { Success = true };
    }
}
