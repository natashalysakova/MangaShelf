using MangaShelf.BL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MangaShelf.Infrastructure.Installer;

public class AuthorService : IAuthorService
{
    private readonly ILogger<AuthorService> _logger;
    private readonly IAuthorRepository _authorRepository;
    public AuthorService(ILogger<AuthorService> logger, IAuthorRepository authorRepository)
    {
        _logger = logger;
        _authorRepository = authorRepository;
    }

    public async Task<ICollection<Author>> GetByNames(IEnumerable<string> authors, bool createIfNotExists = false)
    {
        var result = new List<Author>();

        foreach (var author in authors)
        {
            var authorEntity = await _authorRepository.GetByName(author);
            if (authorEntity is null && createIfNotExists)
            {
                authorEntity = new Author() { Name = author };
                authorEntity = await _authorRepository.Add(authorEntity);
            }
            result.Add(authorEntity!);
        }
        return result;
    }
}
