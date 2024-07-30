namespace API.Data.Entities
{
    public class NumberOfDocumentsByStatus
    {
        public required DocumentStatus Status { get; set; }
        public required int Total { get; set; }
    }
}
