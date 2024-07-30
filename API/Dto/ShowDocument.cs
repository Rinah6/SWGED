using API.Data;

namespace API.Dto
{
    public class ShowDocument
    {
        public string? Id { get; set; }
        public string? Object { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? Auteur { get; set; }
        public string? De { get; set; }
        public string? NIF { get; set; }
        public string? STAT { get; set; }
        public string? Name { get; set; }
        public DateTime? CreationDate { get; set; }
        public DocumentStatus? Status { get; set; }
        public bool WasSendedByASupplier { get; set; }
        public bool IsTheCurrentStepTurn { get; set; }
        public DocumentRole Role { get; set; }
    }
}
