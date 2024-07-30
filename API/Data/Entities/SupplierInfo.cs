namespace API.Data.Entities
{
    public class SupplierInfo
    {
        public string? NIF { get; set; }
        public string? STAT { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}
