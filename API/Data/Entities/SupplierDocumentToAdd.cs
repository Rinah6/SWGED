namespace API.Data.Entities
{
    public class SupplierDocumentToAdd
    {
        public required IFormFile PdfDocument { get; set; }
        public required string Title { get; set; }
        public required string Email { get; set; }
        public required string Object { get; set; }
        public required string Message { get; set; }
        public required string GlobalDynamicFields { get; set; }
        public required List<IFormFile>? Attachements { get; set; }
        public string Site { get; set; }
    }
}
