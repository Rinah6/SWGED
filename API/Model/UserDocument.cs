using System.ComponentModel.DataAnnotations.Schema;
using API.Data;

namespace API.Model
{
    public class UserDocument
    {
        public int Id { get; set; }

        [ForeignKey(nameof(UserId))]
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public string DocumentId { get; set; }
        public virtual Document Document { get; set; }

        public int Step { get; set; } = 0;
        public required DocumentRole Role { get; set; }
        public string? Color { get; set; }
        public string? Message { get; set; }
        public byte[]? Signature { get; set; }
        public byte[]? Paraphe { get; set; }

        public DateTime? ProcessingDate { get; set; }
        public bool IsTheCurrentStepTurn { get; set; } = false;

        public virtual List<Field> Fields { get; set; }
    }
}
