namespace API.Data.Entities
{
    public class UserToUpdate
    {
        public required string Username { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required int Role { get; set; }
        public required string ProjectId { get; set; }
        public string? Sites { get; set; }
    }
}
