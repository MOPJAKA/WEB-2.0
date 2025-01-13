using Grpc.Core;
using WEB.API.Models;
using WEB.API.Repository.Interfaces;
using WEB.API.Protos.Publishers;
using WEB.API.DTO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WEB.API.Repository.Implementation;

public class PublishersServiceImpl : PublishersService.PublishersServiceBase
{
    private readonly IPublishersRepository _publishersRepository;
    private readonly ICacheService _cacheService; // Inject cache service
    private readonly IConfiguration _configuration; // Inject configuration

    public PublishersServiceImpl(IPublishersRepository publishersRepository, ICacheService cacheService, IConfiguration configuration)
    {
        _publishersRepository = publishersRepository;
        _cacheService = cacheService;
        _configuration = configuration;
    }

    // Method to get all publishers with caching
    public override async Task<GetAllPublishersResponse> GetAllPublishers(PublishersEmptyRequest request, ServerCallContext context)
    {
        var cacheKey = CacheService.GenerateCacheKey(nameof(GetAllPublishers));

        // Check if caching is enabled and if data is in cache
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cachedPublishers = await _cacheService.GetAsync<IEnumerable<Publisher>>(cacheKey);
            if (cachedPublishers != null)
            {
                return new GetAllPublishersResponse { Publishers = { cachedPublishers } };
            }
        }

        // If not in cache, retrieve from repository
        var publishers = await _publishersRepository.GetAllAsync();
        var response = new GetAllPublishersResponse();
        response.Publishers.AddRange(publishers.Select(p => new Publisher
        {
            Id = p.Id,
            Name = p.Name
        }));

        // Store in cache if enabled
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
            await _cacheService.SetAsync(cacheKey, response.Publishers, cacheTimeout);
        }

        return response;
    }

    // Method to get a publisher by ID with caching
    public override async Task<GetPublisherResponse> GetPublisher(GetPublisherRequest request, ServerCallContext context)
    {
        var cacheKey = CacheService.GenerateCacheKey(nameof(GetPublisher), request.Id);

        // Check if caching is enabled and if data is in cache
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cachedPublisher = await _cacheService.GetAsync<Publisher>(cacheKey);
            if (cachedPublisher != null)
            {
                return new GetPublisherResponse
                {
                    Publisher = cachedPublisher
                };
            }
        }

        // If not in cache, retrieve from repository
        var publisher = await _publishersRepository.GetAsync(request.Id);
        if (publisher == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Publisher not found"));
        }

        // Store in cache if enabled
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
            await _cacheService.SetAsync(cacheKey, new Publisher
            {
                Id = publisher.Id,
                Name = publisher.Name
            }, cacheTimeout);
        }

        return new GetPublisherResponse
        {
            Publisher = new Publisher
            {
                Id = publisher.Id,
                Name = publisher.Name
            }
        };
    }

    // Method to create a publisher with cache invalidation
    public override async Task<CreatePublisherResponse> CreatePublisher(CreatePublisherRequest request, ServerCallContext context)
    {
        var publisherDto = new PublishersCreateDTO
        {
            Name = request.Name
        };

        var createdPublisher = await _publishersRepository.CreateAsync(publisherDto);

        // Cache invalidation after creation (optional, depending on use case)
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cacheKey = CacheService.GenerateCacheKey(nameof(GetAllPublishers)); // Invalidating all publishers list cache
            await _cacheService.RemoveAsync(cacheKey);
        }

        return new CreatePublisherResponse
        {
            Publisher = new Publisher
            {
                Id = createdPublisher.Id,
                Name = createdPublisher.Name
            }
        };
    }

    // Method to update a publisher with cache invalidation
    public override async Task<UpdatePublisherResponse> UpdatePublisher(UpdatePublisherRequest request, ServerCallContext context)
    {
        var publisherDto = new PublishersUpdateDTO
        {
            Name = request.Name
        };

        var updatedPublisher = await _publishersRepository.UpdateAsync(request.Id, publisherDto);
        if (updatedPublisher == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Publisher not found"));
        }

        return new UpdatePublisherResponse
        {
            Publisher = new Publisher
            {
                Id = updatedPublisher.Id,
                Name = updatedPublisher.Name
            }
        };
    }

    // Method to delete a publisher with cache invalidation
    public override async Task<DeletePublisherResponse> DeletePublisher(DeletePublisherRequest request, ServerCallContext context)
    {
        var success = await _publishersRepository.DeleteAsync(request.Id);
        if (!success)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Publisher not found"));
        }

        return new DeletePublisherResponse { Success = true };
    }
}
