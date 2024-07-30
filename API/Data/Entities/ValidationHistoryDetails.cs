namespace API.Data.Entities
{
    public class ValidationHistoryDetails
    {
        public required string Username { get; set; }
        public required DateTime CreationDate { get; set; }
        public string? Comment { get; set; }
    }
}
