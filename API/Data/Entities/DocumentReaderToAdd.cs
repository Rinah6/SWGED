namespace API.Data.Entities
{
    public class DocumentReaderToAdd
    {
        public required string Email { get; set; }
        public required Guid DocumentId { get; set; }
    }
}
