using API.Data;

namespace API.Dto
{
    public class DocumentDto
    {
        public string? Code { get; set; }
        public string? DocPasword { get; set; }
        public string? Filename { get; set; }
        public string? Url { get; set; }
        public string? Cc { get; set; }
        public string? Title { get; set; }
        public string? Object { get; set; }
        public string? Message { get; set; }
        public DateTime? CreationDate { get; set; }
        public DocumentStatus? Status { get; set; }
    }
}
