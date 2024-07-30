namespace API.Data.Entities
{
    public class SignedDocumentToVerify
    {
        public required Guid DocumentId { get; set; }
        public required string DigitalSignatureId { get; set; }
    }
}
