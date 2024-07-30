namespace API.Data.Entities
{
    public class ReversementToUpdate
    {
        public required string ReversementId { get; set; }
        public required Guid TomProConnectionId { get; set; }
        public required Guid TomProDatabaseId { get; set; }
        public required string NewLInk { get; set; }
    }
}
