using API.Data;

namespace API.Dto
{
    public class DocumentRecipientsDto
    {
        public required Guid Id { get; set; }
        public required DocumentRole Role { get; set; }
        public required string Message { get; set; }
        public required string Color { get; set; }
        public List<Data.Entities.Field>? Fields { get; set; }
        public DateTime? ProcessingDate { get; set; }
    }
}
