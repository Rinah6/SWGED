namespace API.Data.Entities
{
    public class Supplier
    {
        public string? NIF { get; set; }
        public string? STAT { get; set; }
        public string? MAIL { get; set; }
        public string? CONTACT { get; set; }
        public string? CIN { get; set; }
        public required string Name { get; set; }
        public required Guid ProjectId { get; set; }
    }
}
