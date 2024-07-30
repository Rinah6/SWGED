using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class Attachement
    {
        [Key]
        public Guid Id { get; set; }
        public string Filename { get; set; }
        public string Url { get; set; }
        public DateTime? DeletionDate { get; set; }
        public Guid DocumentId { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }
    }
}
