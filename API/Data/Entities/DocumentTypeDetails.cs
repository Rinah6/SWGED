namespace API.Data.Entities
{
    public class DocumentTypeDetails
    {
        public required string Title { get; set; }
        public required List<DocumentTypeStep> Steps { get; set; }

        public string? Sites { get; set; }
    }
}
