using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MvcMovie.Models
{
    public enum Roles
    {
        Admin,
        User,
        Moderator
    }

    public class Role
    {
        public int Id { get; set; }

        [Required]
        public string RoleName { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }
    }

    public class ApplicationUser
    {
        public int Id { get; set; }
        public string Username { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
