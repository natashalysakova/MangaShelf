using AngleSharp.Dom;
using MangaShelf.BL.Enums;
using MangaShelf.Common.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MangaShelf.BL.Parsers
{
    public class VivatParser : BaseParser
    {
        public VivatParser(ILogger<BaseParser> logger, [FromKeyedServices(HtmlDownloaderKeys.Advanced)] IHtmlDownloader htmlDownloader) : base(logger, htmlDownloader)
        {
        }

        public override string SiteUrl => "https://vivat.com.ua";

        public override string CatalogUrl => "/category/manga/";

        public override string Pagination => "?page={0}";

        public override string VolumeTitleSelector => "h1";

        protected override int? GetAgeRestriction(IDocument document)
        {
            return null;
        }

        protected override string? GetAuthors(IDocument document)
        {
            var autors = GetFromTable(document, "Автор");
            var illustrators = GetFromTable(document, "Ілюстратор");

            if(autors != null && illustrators != null)
            {
                return string.Join(',', autors, illustrators);
            }

            return autors ?? illustrators;
        }

        private string? GetFromTable(IDocument document, string fieldName)
        {
            var characteristics = document.QuerySelector("#characteristics");
            if (characteristics == null) return null;

            // Get all rows - they're nested inside the characteristics div
            // Look for divs that have the label as direct text content
            var allDivs = characteristics.QuerySelectorAll("div");

            foreach (var div in allDivs)
            {
                // Find the exact div that contains just the label text
                var children = div.Children;
                if (children.Length >= 2)
                {
                    // Check if the first child contains our field name
                    var labelElement = children[0];
                    if (labelElement.TextContent?.Trim() == fieldName)
                    {
                        // The value should be in the next cell (second child or further)
                        var valueElement = children.Length > 1 ? children[1] : null;

                        if (valueElement != null)
                        {
                            // Value might be in a link or direct text
                            var link = valueElement.QuerySelector("a");
                            if (link != null)
                            {
                                return link.TextContent?.Trim();
                            }

                            return valueElement.TextContent?.Trim();
                        }
                    }
                }
            }

            return null;
        }

        protected override string GetCover(IDocument document)
        {
            var node = document.QuerySelector(".swiper-slide-active div div picture img");
            var parsedPath = node?.GetAttribute("src") ?? string.Empty;

            var url = SiteUrl + parsedPath;
            return url;
        }

        protected override string? GetDescription(IDocument document)
        {
            var node = document.QuerySelector("#annotationPanel");

            return node?.TextContent?.Trim();
        }

        protected override string? GetISBN(IDocument document)
        {
            return GetFromTable(document, "ISBN");
        }

        protected override bool GetIsPreorder(IDocument document)
        {
            return false;
        }

        protected override string? GetOriginalSeriesName(IDocument document)
        {
            return GetFromTable(document, "Оригінальна назва");
        }

        protected override string GetPublisher(IDocument document)
        {
            return GetFromTable(document, "Видавництво") ?? "Vivat";
        }

        protected override DateTimeOffset? GetReleaseDate(IDocument document)
        {
            var year = GetFromTable(document, "Рік видання");
            if (year != null && int.TryParse(year, out var y))
            {
                if (y == DateTime.Now.Year)
                {
                    return DateTimeOffset.Now;
                }
                else
                {
                    var dateTime = new DateTime(y, 12, 31);
                    var offset = TimeZoneInfo.Local.GetUtcOffset(dateTime);
                    return new DateTimeOffset(dateTime, offset);
                }
            }

            return null;
        }

        protected override DateTimeOffset? GetSaleStartDate(IDocument document)
        {
            return null;
        }

        protected override string GetSeries(IDocument document)
        {
            var series = GetFromTable(document, "Серія");
            if (series == null)
            {
                return base.GetSeries(document);
            }

            return series;
        }

        protected override string GetVolumeTitle(IDocument document)
        {
            var baseName = base.GetVolumeTitle(document);
            var seriesName = GetSeries(document);
            return baseName.Replace(seriesName, "").Trim(new char[] { ' ', ',', '-', '–', '—', '.' });
        }

        protected override int? GetVolumeNumber(IDocument document)
        {
            return base.GetVolumeNumber(document);
        }

        protected override SeriesStatus GetSeriesStatus(IDocument document)
        {
            return SeriesStatus.Unknown;
        }

        protected override VolumeType GetVolumeType(IDocument document)
        {
            return VolumeType.Physical;
        }

        protected override string GetVolumeUrlBlockClass()
        {
            return ".title";
        }
    }
}
