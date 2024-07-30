using API.Data;

namespace API.Dto
{
    public class UserRoleDocumentDto
    {
        public required string Mail { get; set; }
        public required DocumentRole Role { get; set; }
        public required int Step { get; set; }
    }
}
