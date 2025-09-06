using MangaShelf.DAL.System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaShelf.BL.Dto
{
    public class ParserStatusDto
    {
        public Guid Id { get; set; }
        public required string ParserName { get; set; }
        public ParserStatus Status { get; set; }
        public double Progress { get; set; }

        public Guid? RunningJobId { get; set; }
    }
}
