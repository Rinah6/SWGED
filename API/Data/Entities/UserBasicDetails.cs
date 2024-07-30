namespace API.Data.Entities
{
    public class UserBasicDetails
    {
        public required string Id { get; set; }
        public required string Username { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required UserRole Role { get; set; }
    }
}
