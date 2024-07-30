namespace API.Data.Entities
{
    public class SupplierDocumentDetails
    {
        public required string Filename { get; set; }
        public required string Title { get; set; }
        public required string Object { get; set; }
        public required string Message { get; set; }
        public required DateTime CreationDate { get; set; }
        public required bool WasAcknowledged { get; set; }
        public required bool IsTheCurrentStepTurn { get; set; }
        public required DocumentStatus Status { get; set; }
        public string? NIF { get; set; }
        public string? STAT { get; set; }
        public required string Name { get; set; }
        public required bool WasSendedByASupplier { get; set; }
    }
}
