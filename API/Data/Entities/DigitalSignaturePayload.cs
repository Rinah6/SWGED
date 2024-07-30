namespace API.Data.Entities
{
    public class DigitalSignaturePayload
    {
        public required Guid DocumentId { get; set; }
        public required Guid SignatureId { get; set; }
        public required string Token { get; set; }
        public DateTime Now { get; set; } = DateTime.Now;
        public required uint PageIndex { get; set; }
        public required float X { get; set; }
        public required float Y { get; set; }
    }
}
