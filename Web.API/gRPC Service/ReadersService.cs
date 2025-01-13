using Grpc.Core;
using WEB.API.Models;
using WEB.API.Repository.Interfaces;
using WEB.API.Protos.Readers;
using WEB.API.DTO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WEB.API.Repository.Implementation;

public class ReadersServiceImpl : ReadersService.ReadersServiceBase
{
    private readonly IReadersRepository _readersRepository;
    private readonly ICacheService _cacheService; // Inject cache service
    private readonly IConfiguration _configuration; // Inject configuration

    public ReadersServiceImpl(IReadersRepository readersRepository, ICacheService cacheService, IConfiguration configuration)
    {
        _readersRepository = readersRepository;
        _cacheService = cacheService;
        _configuration = configuration;
    }

    public override async Task<GetAllReadersResponse> GetAllReaders(ReadersEmptyRequest request, ServerCallContext context)
    {
        var cacheKey = CacheService.GenerateCacheKey(nameof(GetAllReaders));

        // Check if data is in cache
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cachedReaders = await _cacheService.GetAsync<IEnumerable<Reader>>(cacheKey);
            if (cachedReaders != null)
            {
                return new GetAllReadersResponse { Readers = { cachedReaders } };
            }
        }

        // Retrieve from repository if not in cache
        var readers = await _readersRepository.GetAllAsync();
        var response = new GetAllReadersResponse();
        response.Readers.AddRange(readers.Select(r => new Reader
        {
            Id = r.Id,
            FirstName = r.FirstName,
            LastName = r.LastName,
            BirthDayDate = r.BirthDayDate.ToString("yyyy-MM-dd"),
            Gender = r.Gender.ToString(),
            EducationLevel = r.EducationLevel
        }));

        // Store in cache
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
            await _cacheService.SetAsync(cacheKey, response.Readers, cacheTimeout);
        }

        return response;
    }

    public override async Task<GetReaderResponse> GetReader(GetReaderRequest request, ServerCallContext context)
    {
        var cacheKey = CacheService.GenerateCacheKey(nameof(GetReader), request.Id);

        // Check if data is in cache
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cachedReader = await _cacheService.GetAsync<Reader>(cacheKey);
            if (cachedReader != null)
            {
                return new GetReaderResponse { Reader = cachedReader };
            }
        }

        // Retrieve from repository if not in cache
        var reader = await _readersRepository.GetAsync(request.Id);
        if (reader == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Reader not found"));
        }

        var responseReader = new Reader
        {
            Id = reader.Id,
            FirstName = reader.FirstName,
            LastName = reader.LastName,
            BirthDayDate = reader.BirthDayDate.ToString("yyyy-MM-dd"),
            Gender = reader.Gender.ToString(),
            EducationLevel = reader.EducationLevel
        };

        // Store in cache
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cacheTimeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("Redis:CacheTimeoutInSeconds"));
            await _cacheService.SetAsync(cacheKey, responseReader, cacheTimeout);
        }

        return new GetReaderResponse { Reader = responseReader };
    }

    public override async Task<CreateReaderResponse> CreateReader(CreateReaderRequest request, ServerCallContext context)
    {
        DateOnly birthDate = DateOnly.ParseExact(request.BirthDayDate, "yyyy-MM-dd");
        char gender = char.Parse(request.Gender);

        var readerDto = new ReadersCreateDTO
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            BirthDayDate = birthDate,
            Gender = gender,
            EducationLevel = request.EducationLevel
        };

        var createdReader = await _readersRepository.CreateAsync(readerDto);

        // Invalidate cache
        if (_configuration.GetValue<bool>("Redis:Enabled"))
        {
            var cacheKey = CacheService.GenerateCacheKey(nameof(GetAllReaders));
            await _cacheService.RemoveAsync(cacheKey);
        }

        return new CreateReaderResponse
        {
            Reader = new Reader
            {
                Id = createdReader.Id,
                FirstName = createdReader.FirstName,
                LastName = createdReader.LastName,
                BirthDayDate = createdReader.BirthDayDate.ToString("yyyy-MM-dd"),
                Gender = createdReader.Gender.ToString(),
                EducationLevel = createdReader.EducationLevel
            }
        };
    }

    public override async Task<UpdateReaderResponse> UpdateReader(UpdateReaderRequest request, ServerCallContext context)
    {
        DateOnly birthDate = DateOnly.ParseExact(request.BirthDayDate, "yyyy-MM-dd");
        char gender = char.Parse(request.Gender);

        var readerDto = new ReadersUpdateDTO
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            BirthDayDate = birthDate,
            Gender = gender,
            EducationLevel = request.EducationLevel
        };

        var updatedReader = await _readersRepository.UpdateAsync(request.Id, readerDto);
        if (updatedReader == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Reader not found"));
        }

        return new UpdateReaderResponse
        {
            Reader = new Reader
            {
                Id = updatedReader.Id,
                FirstName = updatedReader.FirstName,
                LastName = updatedReader.LastName,
                BirthDayDate = updatedReader.BirthDayDate.ToString("yyyy-MM-dd"),
                Gender = updatedReader.Gender.ToString(),
                EducationLevel = updatedReader.EducationLevel
            }
        };
    }

    public override async Task<DeleteReaderResponse> DeleteReader(DeleteReaderRequest request, ServerCallContext context)
    {
        var success = await _readersRepository.DeleteAsync(request.Id);
        if (!success)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Reader not found"));
        }

        return new DeleteReaderResponse { Success = true };
    }
}