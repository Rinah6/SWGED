using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class DocumentLink
    {
        [Key]
        public string CodeLink { get; set; }
        public DateTime ExpiredDate { get; set; }
        public Guid CodeDocument { get; set; }

        [ForeignKey(nameof(CodeDocument))]
        public virtual Document Document { get; set; }
    }
}
