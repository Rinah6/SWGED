namespace API.Data.Entities
{
    public class SupplierDocument
    {
        public required Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Object { get; set; }
        public required string Message { get; set; }
        public string? NIF { get; set; }
        public string? STAT { get; set; }
        public required string Name { get; set; }
        public required string De { get; set; }
        public required string Pour { get; set; }
        public required DateTime CreationDate { get; set; }
        public required DocumentStatus Status { get; set; }
        public bool WasSendedByASupplier { get; set; }
        public required string Site { get; set; }
    }
}
