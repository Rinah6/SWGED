namespace API.Data.Entities
{
    public class JustificatifToUpdate
    {
        public required string JustificatifId { get; set; }
        public required Guid TomProConnectionId { get; set; }
        public required Guid TomProDatabaseId { get; set; }
        public required string NewLInk { get; set; }
    }
}
