using API.Data;

namespace API.Dto
{
    public class UserDto
    {
        public Guid? Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Guid? ProjectId { get; set; }
        public string? Nproject { get; set; }
        public string? Csubscription { get; set; }
        public UserRole? Role { get; set; }
        public string? TransfertMail { get; set; }
    }
}
