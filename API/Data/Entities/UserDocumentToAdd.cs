namespace API.Data.Entities
{
    public class UserDocumentToAdd
    {
        public required Guid UserId { get; set; }
        public required Guid DocumentId { get; set; }
        public required int Step { get; set; }
        public required DocumentRole Role { get; set; }
        public string? Color { get; set; }
        public required string Message { get; set; }
        public required bool IsTheCurrentStepTurn { get; set; }
        public DateTime? ProcessingDate { get; set; }
        public List<Field>? Fields { get; set; }
    }
}
