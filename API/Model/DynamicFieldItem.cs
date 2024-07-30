using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class DynamicFieldItem
    {
        [Key]
        public Guid Id { get; set; }
        public required string Value { get; set; }
        public required Guid DynamicFieldId { get; set; }
        public DateTime? DeletionDate { get; set; }

        [ForeignKey(nameof(DynamicFieldId))]
        public virtual DynamicField? DynamicField { get; set; }
    }
}
