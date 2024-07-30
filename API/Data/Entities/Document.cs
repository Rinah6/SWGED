namespace API.Data.Entities
{
    public class Document
    {
        public required Guid Id { get; set; }
        public required string Title { get; set; }
        public required DateTime CreationDate { get; set; }
        public required string Sender { get; set; }
        public required DocumentRole Role { get; set; }
        public required bool IsTheCurrentStepTurn { get; set; }
        public required string Site { get; set; }
    }
}
