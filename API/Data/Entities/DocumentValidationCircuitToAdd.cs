namespace API.Data.Entities
{
    public class DocumentValidationCircuitToAdd
    {
        public required IFormFile DocumentFile { get; set; }
        public required string Title { get; set; }
        public required string GlobalDynamicFields { get; set; }
        public required string Recipients { get; set; }
        public List<IFormFile>? Attachements { get; set; }
        public required string Object { get; set; }
        public required string Message { get; set; }
        public required string RSF { get; set; }
        public string Site { get; set; }
    }
}
