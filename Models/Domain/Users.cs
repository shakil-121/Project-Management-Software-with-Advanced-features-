
using Microsoft.AspNetCore.Identity;
using FastPMS.Models;
namespace FastPMS.Models.Domain
{
    public class Users:IdentityUser
    {
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }

        public virtual ICollection<IdentityUserRole<string>> Roles { get; set; }
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public bool IsActive { get; internal set; }
    }
}
