namespace API.Data.Entities
{
    public class SupplierDetails
    {
        public string Id { get; set; }
        public string? NIF { get; set; }
        public string? STAT { get; set; }
        public string? MAIL { get; set; }
        public string? CONTACT { get; set; }
        public string? CIN { get; set; }
        public required string Name { get; set; }
        public required DateTime CreationDate { get; set; }
    }
}
