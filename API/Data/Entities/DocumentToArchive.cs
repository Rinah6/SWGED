namespace API.Data.Entities
{
    public class DocumentToArchive
    {
        public required IFormFile File { get; set; }
        public required string Title { get; set; }
        public required string Object { get; set; }
        public required string Message { get; set; }
        public required string GlobalDynamicFields { get; set; }
        public List<IFormFile>? Attachements { get; set; }
        public required string RSF { get; set; }
        public required string Site { get; set; }
    }
}
