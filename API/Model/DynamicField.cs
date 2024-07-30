using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Data;

namespace API.Model
{
    public class DynamicField
    {
        [Key]
        public Guid Id { get; set; }
        public required string Label { get; set; }
        public required bool IsRequired { get; set; }
        public required DynamicFieldType Type { get; set; }
        public required Guid ProjectId { get; set; }
        public DateTime? DeletionDate { get; set; }
        public required bool IsGlobal { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public virtual Project? Project { get; set; }

        public virtual List<DynamicFieldItem>? Items { get; set; }
        public virtual List<DocumentDynamicField>? Values { get; set; }
    }
}
