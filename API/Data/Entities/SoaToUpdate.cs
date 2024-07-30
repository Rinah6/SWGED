namespace API.Data.Entities
{
    public class SoaToUpdate
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime? DeletionDate { get; set; }

        public DateTime? CreationDate { get; set; }
    }
}
