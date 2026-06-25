using MangaShelf.DAL;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.Migration.DataCorrections;


public class Correction_20260625_001_PublisherAlternativeNames : IDataCorrection
{
    private readonly ILogger<Correction_20260625_001_PublisherAlternativeNames> _logger;

    public Correction_20260625_001_PublisherAlternativeNames(ILogger<Correction_20260625_001_PublisherAlternativeNames> logger)
    {
        _logger = logger;
    }

    public async Task ApplyCorrection(MangaDbContext context)
    {
        var correctionList = new Dictionary<string, string[]>
        {

            { "Nasha Idea", ["Наша Ідея", "ТАК"] },
            { "Mal'opus", ["MAL'OPUS"] },
            { "Artbooks", ["Артбукс"] },
            { "Molfar Comics", ["Molfar", "Видавництво Molfar", "Molfar"] }

        };

        foreach (var publisher  in correctionList)
        {
            var publisherEntity = await context.Publishers.Where(p => p.Name == publisher.Key).FirstOrDefaultAsync();

            if (publisherEntity != null)
            {
                if(!publisherEntity.AlternativeNames.Any())
                {
                    publisherEntity.AlternativeNames = publisher.Value.ToList();
                    _logger.LogInformation($"Updated publisher '{publisherEntity.Id}':'{publisher.Key}' with alternative names: {string.Join(", ", publisher.Value)}");
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
