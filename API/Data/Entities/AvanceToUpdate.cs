namespace API.Data.Entities
{
    public class AvanceToUpdate
    {
        public required string AvanceId { get; set; }
        public required Guid TomProConnectionId { get; set; }
        public required Guid TomProDatabaseId { get; set; }
        public required string NewLInk { get; set; }
    }
}
