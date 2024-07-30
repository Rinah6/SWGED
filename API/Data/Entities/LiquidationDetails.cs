namespace API.Data.Entities
{
    public class LiquidationDetails
    {
        public required string Id { get; set; }
        public required string Code { get; set; }
        public required string Designation { get; set; }
        public required decimal Montant { get; set; }
        public required string TypePiece { get; set; }
        public string? Lien { get; set; }
    }
}
