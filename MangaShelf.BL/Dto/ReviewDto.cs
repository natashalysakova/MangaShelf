namespace MangaShelf.BL.Dto
{
    public class ReviewDto
    {
        public string UserName { get; set; }
        public int  Rating { get; set; }
        public string? Review { get; set; }
        public string? Id { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}
