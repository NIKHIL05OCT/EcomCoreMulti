using Microsoft.AspNetCore.Identity;

namespace EcomAuthentication.Models
{
    public class ApiUser : IdentityUser<int>
    {
        public string? FullName { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? LastLoginDevice { get; set; }
        public bool? IsOnline { get; set; }
        public string? OnlineDevice { get; set; }
        public bool? IsIPRestricted { get; set; }
        public string? AccessibleIP { get; set; }
        public string? UserType { get; set; }
        public bool IsActive { get; set; }
    }
}
