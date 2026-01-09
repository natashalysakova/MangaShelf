using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaShelf.BL.Dto
{
    public class ReviewDto
    {
        public string UserName { get; set; }
        public int  Rating { get; set; }
        public string? Review { get; set; }
    }
}
