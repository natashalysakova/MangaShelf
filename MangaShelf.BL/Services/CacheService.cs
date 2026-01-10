using MangaShelf.BL.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaShelf.BL.Services
{
    public class CacheService(IMemoryCache memoryCache) : ICacheService
    {
        public IEnumerable<string> GetSearchAutoComplete()
        {
            return memoryCache.Get("Search") as IEnumerable<string> ?? Enumerable.Empty<string>();
        }
    }
}
