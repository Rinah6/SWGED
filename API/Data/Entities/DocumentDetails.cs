namespace API.Data.Entities
{
    public class DocumentDetails
    {
        public required string Filename { get; set; }
        public required string Title { get; set; }
        public required string Object { get; set; }
        public required string Message { get; set; }
        public required DateTime CreationDate { get; set; }
        public required DocumentStatus Status { get; set; }
        public required bool IsTheCurrentStepTurn { get; set; }
        public required bool HasSign { get; set; }
        public required bool HasParaphe { get; set; }
        public string? PhysicalLocation { get; set; }
        public required bool IsTheCurrentUserTheSender { get; set; }
        public required string Site { get; set; }
    }
}
