
using Microsoft.AspNetCore.Identity;

namespace FastPMS.Models.Domain
{
    public class Users:IdentityUser
    {
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }
    }
}
