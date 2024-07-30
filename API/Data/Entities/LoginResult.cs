namespace API.Data.Entities
{
    public class LoginResult
    {
        public required Guid Id { get; set; }
        public required UserRole RoleId { get; set; }
        public required string Password { get; set; }
    }
}
