using Microsoft.AspNetCore.Identity;

namespace EcomAuthentication.Models
{
    public class ApiRole : IdentityRole<int>
    {
        public string? Description { get; set; }
    }
}
