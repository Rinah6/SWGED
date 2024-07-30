using API.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserRole RoleId { get; set; } = UserRole.User;
        public Guid? CreatedBy { get; set; }

        public Guid? ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public virtual Project? Project { get; set; }

        public virtual List<UserDocument>? UsersDocuments { get; set; }
        public DateTime? DeletionDate { get; set; }

        public string? Sites { get; set; }
    }
}
