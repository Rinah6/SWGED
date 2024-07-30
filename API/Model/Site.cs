using System.ComponentModel.DataAnnotations;

namespace API.Model
{
    public class Site
    {
        [Key]
        public required Guid Id { get; set; }
        public required string SiteId { get; set; }
        public required string Name { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? DeletionDate { get; set; }

    }
}
