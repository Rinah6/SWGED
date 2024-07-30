namespace API.Data.Entities
{
    public class DocumentTypeStep
    {
        public required string Id { get; set; }
        public required int StepNumber { get; set; }
        public string? ProcessingDescription { get; set; }
        public required double ProcessingDuration { get; set; }
        public required List<ValidatorDetails> Validators { get; set; }
    }
}
