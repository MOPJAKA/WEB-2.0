using Grpc.Core;
using WEB.API.Models;
using WEB.API.Repository.Interfaces;
using WEB.API.Protos;
using System.Linq;
using System.Threading.Tasks;

public class AuthorsServiceImpl : AuthorService.AuthorServiceBase
{
    private readonly IAuthorsRepository _authorsRepository;

    public AuthorsServiceImpl(IAuthorsRepository authorsRepository)
    {
        _authorsRepository = authorsRepository;
    }

    public override async Task<GetAllAuthorsResponse> GetAllAuthors(EmptyRequest request, ServerCallContext context)
    {
        var authors = await _authorsRepository.GetAllAsync();
        var response = new GetAllAuthorsResponse();
        response.Authors.AddRange(authors.Select(a => new Author
        {
            Id = a.Id,
            FirstName = a.FirstName,
            LastName = a.LastName
        }));
        return response;
    }

    public override async Task<GetAuthorResponse> GetAuthor(GetAuthorRequest request, ServerCallContext context)
    {
        var author = await _authorsRepository.GetAsync(request.Id);
        if (author == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Author not found"));
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

    //public override async Task<CreateAuthorResponse> CreateAuthor(CreateAuthorRequest request, ServerCallContext context)
    //{
    //    var authorDto = new CreateAuthorDTO
    //    {
    //        FirstName = request.FirstName,
    //        LastName = request.LastName
    //    };
    //    var createdAuthor = await _authorsRepository.CreateAsync(authorDto);
    //    return new CreateAuthorResponse
    //    {
    //        Author = new Author
    //        {
    //            Id = createdAuthor.Id,
    //            FirstName = createdAuthor.FirstName,
    //            LastName = createdAuthor.LastName
    //        }
    //    };
    //}
    //
    //public override async Task<UpdateAuthorResponse> UpdateAuthor(UpdateAuthorRequest request, ServerCallContext context)
    //{
    //    var authorDto = new UpdateAuthorDTO
    //    {
    //        FirstName = request.FirstName,
    //        LastName = request.LastName
    //    };
    //    var updatedAuthor = await _authorsRepository.UpdateAsync(request.Id, authorDto);
    //    if (updatedAuthor == null)
    //    {
    //        throw new RpcException(new Status(StatusCode.NotFound, "Author not found"));
    //    }
    //    return new UpdateAuthorResponse
    //    {
    //        Author = new Author
    //        {
    //            Id = updatedAuthor.Id,
    //            FirstName = updatedAuthor.FirstName,
    //            LastName = updatedAuthor.LastName
    //        }
    //    };
    //}

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
