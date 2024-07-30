namespace API.Data.Entities
{
    public class NewDocumentRedirection
    {
        public required Guid TargetDocumentStepId { get; set; }
        public required string Message { get; set; }
    }
}
