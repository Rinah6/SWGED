using System.ComponentModel.DataAnnotations;
using API.Data;

namespace API.Model
{
    public class Document
    {
        [Key]
        public Guid Id { get; set; }
        public string Filename { get; set; }
        public required string OriginalFilename { get; set; }
        public string Url { get; set; }
        public required string Title { get; set; }
        public required string Object { get; set; }
        public required string Message { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public required DocumentStatus Status { get; set; }
        public Guid SenderId { get; set; }
        public string? PhysicalLocation { get; set; } = "";

        public virtual List<Attachement>? Attachements { get; set; }
        public virtual List<UserDocument>? UsersDocuments { get; set; }
        public virtual List<DocumentDynamicField>? DocumentDetailValues { get; set; }
        public virtual List<DocumentLink>? DocumentLinks { get; set; }
        public bool RSF { get; set; }
        public required string Site { get; set; }
    }
}
