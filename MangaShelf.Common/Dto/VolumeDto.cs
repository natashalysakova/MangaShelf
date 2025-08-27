namespace MangaShelf.Common.Dto
{
    public class VolumeDto
    {
        public Guid Id{ get; set; }
        public string SeriesName { get; set; }
        public string VolumeTitle { get; set; }
        public string CoverImageUrl { get; set; }
        public int VolumeNumber { get; set; }
    }
}