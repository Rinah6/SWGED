namespace API.Data.Entities
{
    public class RecipientsStep
    {
        public required int StepNumber { get; set; }
        public string? ProcessingDescription { get; set; }
        public required float ProcessingDuration { get; set; }
        public required DocumentRole Role { get; set; }
        public required List<Guid> UsersId { get; set; }
        public required string Color { get; set; }
        public required string Message { get; set; }
        public List<Field>? Fields { get; set; }
    }
}
