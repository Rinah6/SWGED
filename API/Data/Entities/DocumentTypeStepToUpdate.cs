namespace API.Data.Entities
{
    public class DocumentTypeStepToUpdate
    {
        public required int StepNumber { get; set; }
        public string? ProcessingDescription { get; set; }
        public required double ProcessingDuration { get; set; }
    }
}
