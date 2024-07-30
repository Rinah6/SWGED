using System.ComponentModel.DataAnnotations;

namespace API.Model
{
    public class Soa
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime? DeletionDate { get; set; }

        public DateTime? CreationDate { get; set; }
    }
}
