namespace API.Dto
{
    public class AllUserDocumentDto
    {
        public required IFormFile Document { get; set; }
        public required string Title { get; set; }
        public required string Object { get; set; }
        public required string Message { get; set; }
        public required string GlobalDynamicFields { get; set; }
        public required string Recipients { get; set; }
        public List<IFormFile>? Attachements { get; set; }
    }
}
