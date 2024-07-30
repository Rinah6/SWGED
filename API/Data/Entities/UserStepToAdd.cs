namespace API.Data.Entities
{
    public class UserStepToAdd
    {
        public required int StepNumber { get; set; }
        public string? ProcessingDescription { get; set; }
        public required double ProcessingDuration { get; set; }
        public required string Color { get; set; }
        public required List<Guid> UsersId { get; set; }
    }
}
