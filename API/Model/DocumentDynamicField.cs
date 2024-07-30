using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class DocumentDynamicField
    {
        [Key]
        public required Guid DynamicFieldId { get; set; }
        public required Guid DocumentId { get; set; }

        public required string Value { get; set; }


        [ForeignKey(nameof(DocumentId))]
        public virtual Document? Document { get; set; }

        [ForeignKey(nameof(DynamicFieldId))]
        public virtual DynamicField? DynamicField { get; set; }
    }
}
