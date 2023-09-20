using System.ComponentModel.DataAnnotations;

namespace EcomAuthentication.Models
{

    public class LoginModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }

    public class RoleModel
    {
        [Required(ErrorMessage = "Role Name is required")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }
    }

    public class UserRoleModel
    {
        [Required(ErrorMessage = "Role Name is required")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        public string? UserName { get; set; }
    }
}
