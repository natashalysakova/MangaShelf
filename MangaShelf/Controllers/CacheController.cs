using MangaShelf.Cache;
using Microsoft.AspNetCore.Mvc;

namespace MangaShelf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheController : ControllerBase
    {
        private readonly ICacheProvider _cacheProvider;

        public CacheController(ICacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        [HttpPost]
        public async Task<IActionResult> RebuildCache(CancellationToken cancellationToken)
        {
            await _cacheProvider.RebuildCache(cancellationToken);
            return Ok();
        }
    }
}
