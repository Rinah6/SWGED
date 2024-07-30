namespace API.Data.Entities
{
    public class NewDocumentDetails
    {
        public required IFormFile DocumentFile { get; set; }
        public required string Title { get; set; }
        public List<IFormFile>? Attachements { get; set; }
        public required string Object { get; set; }
        public required string Message { get; set; }
        public required bool RSF { get; set; }
        public string Site { get; set; }
    }
}
