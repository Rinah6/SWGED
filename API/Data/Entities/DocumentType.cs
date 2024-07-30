namespace API.Data.Entities
{
    public class DocumentType
    {
        public required string Id { get; set; }
        public required string Title { get; set; }

        public string? Sites { get; set; }

    }
}
