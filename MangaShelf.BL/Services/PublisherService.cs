using AngleSharp.Dom;
using MangaShelf.BL.Interfaces;
using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Infrastructure.Installer
{
    public class PublisherService : IPublisherService
    {
        private readonly IPublisherRepository _publisherRepository;
        private readonly ICountryRepository _countryRepository;

        public PublisherService(ILogger<Publisher> logger, IPublisherRepository publisherRepository, ICountryRepository countryRepository)
        {
            _publisherRepository = publisherRepository;
            _countryRepository = countryRepository;
        }

        public async Task<Publisher> CreateFromParsedVolumeInfo(ParsedInfo volumeInfo)
        {

            var country = await _countryRepository.GetByCountryCodeAsync(volumeInfo.countryCode) ?? await _countryRepository.GetByCountryCodeAsync("uk");
            var publisher = new Publisher()
            {
                Name = volumeInfo.publisher,
                Country = country,
                Url = new Uri(volumeInfo.url).ToString()
            };

            await _publisherRepository.Add(publisher);
            return publisher;
        }

        public async Task<Publisher?> GetByName(string publisher)
        {
            return await _publisherRepository.GetByName(publisher);
        }
    }
}