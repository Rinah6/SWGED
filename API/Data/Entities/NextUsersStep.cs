namespace API.Data.Entities
{
    public class NextUsersStep
    {
        public required Guid Id { get; set; }
        public required List<NextUserStepDetails> Users { get; set; }
    }
}
