namespace API.Data.Entities
{
    public class DocumentToSign
    {
        public required IFormFile DocumentFile { get; set; }
        public required string Title { get; set; }
        public required string Object { get; set; }
        public required string Message { get; set; }
        public required string GlobalDynamicFields { get; set; }
        public required string SignatureId { get; set; }
        public required string Token { get; set; }
        public required string FieldDetails { get; set; }
        public required List<IFormFile>? Attachements { get; set; }
        public required string RSF { get; set; }
        public required string Site { get; set; }
    }
}
