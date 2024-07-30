namespace API.Data.Entities
{
    public class UserLastConnection
    {
        public required DateTime CreationDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
