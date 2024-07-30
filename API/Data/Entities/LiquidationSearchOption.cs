namespace API.Data.Entities
{
    public class LiquidationSearchOption
    {
        public required string Code { get; set; }
        public required Guid TomProConnectionId { get; set; }
        public required Guid TomProDatabaseId { get; set; }
    }
}
