namespace API.Dto
{
    public class AttachementDto
    {
        public string? Filename { get; set; }
        public required List<IFormFile> Attachements { get; set; }
    }
}
