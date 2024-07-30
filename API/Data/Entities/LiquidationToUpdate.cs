namespace API.Data.Entities
{
    public class LiquidationToUpdate
    {
        public required string LiquidationId { get; set; }
        public required Guid TomProConnectionId { get; set; }
        public required Guid TomProDatabaseId { get; set; }
        public required string NewLInk { get; set; }
    }
}
