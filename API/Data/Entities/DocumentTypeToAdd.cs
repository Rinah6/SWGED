namespace API.Data.Entities
{
    public class DocumentTypeToAdd
    {
        public required string Title { get; set; }
        public required List<DocumentTypeStepToAdd> Steps { get; set; }

        public string? Sites { get; set; }
    }
}
