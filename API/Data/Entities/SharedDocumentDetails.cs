namespace API.Data.Entities
{
    public class SharedDocumentDetails
    {
        public required string Filename { get; set; }
        public required string Title { get; set; }
        public required string Object { get; set; }
        public required string Message { get; set; }
        public required DateTime CreationDate { get; set; }
        public required DocumentStatus Status { get; set; }
    }
}
