namespace API.Data.Entities
{
    public class JustificatifDetails
    {
        public required string Id { get; set; }
        public required string Code { get; set; }
        public required decimal Montant { get; set; }
        public string? Commentaire { get; set; }
    }
}
