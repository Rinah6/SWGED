namespace API.Data.Entities
{
    public class UserDocument
    {
        public required Guid UserId { get; set; }
        public required int Step { get; set; }
        public required DocumentRole Role { get; set; }
        public string? Color { get; set; }
        public required string Message { get; set; }
        public required bool IsTheCurrentStepTurn { get; set; }
        public byte[]? Signature { get; set; }
        public byte[]? Paraphe { get; set; }
        public DateTime? ProcessingDate { get; set; }
        public required List<Field> Fields { get; set; }
    }
}
