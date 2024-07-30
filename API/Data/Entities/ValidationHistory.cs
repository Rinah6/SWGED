namespace API.Data.Entities
{
    public class ValidationHistory
    {
        public required Guid FromUserId { get; set; }
        public Guid? ToDocumentStepId { get; set; }
        public required Guid DocumentId { get; set; }
        public string? Comment { get; set; }
        public required DocumentActionType ActionType { get; set; }
        public required DateTime CreationDate { get; set; }
    }
}
