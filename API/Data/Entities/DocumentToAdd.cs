namespace API.Data.Entities
{
    public class DocumentToAdd
    {
        public required Guid Id { get; set; }
        public required string Filename { get; set; }
        public required string OriginalFilename { get; set; }
        public required string Url { get; set; }
        public required string Title { get; set; }
        public required string Object { get; set; }
        public required string Message { get; set; }
        public required DocumentStatus Status { get; set; }
        public required Guid SenderId { get; set; }
        public required bool RSF { get; set; }
        public required string Site { get; set; }
    }
}
