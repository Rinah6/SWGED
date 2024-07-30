using System.ComponentModel.DataAnnotations;

namespace API.Model
{
    public class UsersProjectsSites
    {
        public int Id { get; set; }

        public Guid? UserId { get; set; }

        public string? ProjectId { get; set; }

        public string? Site { get; set; }
    }
}
