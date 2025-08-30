using MangaShelf.BL.Interfaces;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;
using MangaShelf.BL.Mappers;
using MangaShelf.BL.Dto;

namespace MangaShelf.BL.Services;

public class PublisherService : IPublisherService
{
    private readonly IPublisherDomainService _publisherRepository;
    private readonly ICountryDomainService _countryRepository;

    public PublisherService(ILogger<Publisher> logger, IPublisherDomainService publisherRepository, ICountryDomainService countryRepository)
    {
        _publisherRepository = publisherRepository;
        _countryRepository = countryRepository;
    }

    public async Task<PublisherDto?> GetByName(string publisherName)
    {
        var publisher = await _publisherRepository.GetByName(publisherName);
        return publisher?.ToDto();
    }
}