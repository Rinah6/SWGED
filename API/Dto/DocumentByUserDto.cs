namespace API.Dto
{
    public class DocumentByUserDto
    {
        public int? Id { get; set; }
        public Guid? UserId { get; set; }
        public string UserEmail { get; set; }
        public string? DocumentId { get; set; }
        public DocumentDto? Document { get; set; }
        public int? Step { get; set; } = 0;
        public string? Role { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string? Cc { get; set; }
        public string Site { get; set; }
    }
}
