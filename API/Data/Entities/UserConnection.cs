namespace API.Data.Entities
{
    public class UserConnection
    {
        public required Guid Id { get; set; }
        public required string LastName { get; set; }
        public required string FirstName { get; set; }
        public required string Username { get; set; }
        public required DateTime CreationDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
