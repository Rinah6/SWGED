namespace API.Data.Entities
{
    public class ProcessingCircuitUserToAdd
    {
        public required Guid UserId { get; set; }
        public required string DocumentId { get; set; }
        public required int Step { get; set; }
        public required int Role { get; set; }
    }
}