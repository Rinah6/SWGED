namespace API.Data.Entities
{
    public class UserConnectionHistory
    {
        public required long Id { get; set; }
        public required DateTime CreationDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
