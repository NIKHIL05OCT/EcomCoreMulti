using EcomAuthentication.AppCodes;
using EcomAuthentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcomAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApiUser> _userManager;
        private readonly RoleManager<ApiRole> _roleManager;
        private readonly SignInManager<ApiUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthenticationController(UserManager<ApiUser> userManager, RoleManager<ApiRole> roleManager, IConfiguration configuration, SignInManager<ApiUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {

                //var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, lockoutOnFailure: false);


                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GetToken(authClaims);

                return Ok(new
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo,
                    FullName = user.FullName,
                    UserId = user.Id,
                    Roles = userRoles,
                    Abc = 1
                });
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpGet]
        [Route("Logout")]
        public IActionResult Logout()
        {
            ApiResponse response = new ApiResponse();
            response.Message = "Logout Successful!";
            response.Status = "1";
            return Ok(response);
        }

        [HttpPost]
        [Route("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Status = "0", Message = "User already exists!" });

            ApiUser user = new()
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                IsOnline = false,
                IsIPRestricted = false,
                IsActive = model.IsActive
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Status = "0", Message = "User creation failed! Please check user details and try again." });

            return Ok(new ApiResponse { Status = "1", Message = "User created successfully!" });
        }

        [HttpPost]
        [Route("CreateRole")]
        public async Task<IActionResult> CreateRole([FromBody] RoleModel model)
        {
            var roleExists = await _roleManager.FindByNameAsync(model.Name);
            if (roleExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Status = "0", Message = "Role already exists!" });

            ApiRole user = new()
            {
                Name = model.Name,
                Description = model.Description

            };
            var result = await _roleManager.CreateAsync(user);

            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Status = "0", Message = "Role creation failed! Please check user details and try again." });

            return Ok(new ApiResponse { Status = "1", Message = "Role created successfully!" });
        }


        [HttpPost]
        [Route("AddUserRole")]
        public async Task<IActionResult> AddUserRole([FromBody] UserRoleModel model)
        {
            var roleExists = await _roleManager.FindByNameAsync(model.Name);
            if (roleExists == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Status = "0", Message = "Role not found!" });

            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Status = "0", Message = "User not found!" });

            var result = await _userManager.AddToRoleAsync(userExists, model.Name);

            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Status = "0", Message = "Role assigning failed! Please check user details and try again." });
            }
            else
            {
                var user = _userManager.FindByNameAsync(model.UserName);
                var role = _roleManager.FindByNameAsync(model.Name);
                return Ok(new ApiResponse { Status = "1", Message = "Role " + model.Name + " mapped with user " + model.UserName + " successfully!" });
            }
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddMinutes(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        #region User Managements
        [HttpGet]
        [Route("Users")]
        public IActionResult Users()
        {
            ApiResponse response = new ApiResponse();

            var results = _context.UserMasters.ToList();
            if (results.Count > 0)
            {
                response.Message = "Records Found!";
                response.Status = "1";
                response.Data = results;
            }
            else
            {
                response.Message = "Records Not Found!";
                response.Status = "0";
            }
            return Ok(response);
        }

        [HttpGet]
        [Route("User/{id}")]
        public IActionResult UserDetails(int id)
        {
            ApiResponse response = new ApiResponse();

            var results = _context.UserMasters.Find(id);
            if (results != null)
            {
                response.Message = "Records Found!";
                response.Status = "1";
                response.Data = results;
            }
            else
            {
                response.Message = "Records Not Found!";
                response.Status = "0";
            }
            return Ok(response);
        }

        [HttpPut]
        [Route("User")]
        public IActionResult UserUpdate(UserMaster model)
        {
            ApiResponse response = new ApiResponse();

            var existData = _context.UserMasters.Find(model.Id);
            if (existData != null)
            {
                existData.FullName = model.FullName;
                existData.Email = model.Email;
                existData.PhoneNumber = model.PhoneNumber;
                existData.IsActive = model.IsActive;

                var results = _context.Update(existData);
                _context.SaveChanges();
                response.Message = "Records successfully updated!";
                response.Status = "1";
                response.Data = results.Entity;
                return Ok(response);
            }
            else
            {
                response.Message = "Something went wrong, Please try again!";
                response.Status = "0";
                return Ok(response);
            }
        }

        [HttpDelete]
        [Route("User/{id}")]
        public IActionResult UserDelete(int? id)
        {
            ApiResponse response = new ApiResponse();
            var model = _context.UserMasters.Find(id);
            if (model != null)
            {
                try
                {
                    var result = _context.UserMasters.Remove(model);
                    _context.SaveChanges();
                    response.Message = "Record successfully deleted!";
                    response.Status = "1";
                    response.Data = model;
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                    response.Status = "1";
                    response.Data = model;
                    return Ok(response);
                }
            }
            else
            {
                response.Message = "Records Not Found!";
                response.Status = "1";
                return Ok(response);
            }
        }
        #endregion

        #region Roles Managements
        [HttpGet]
        [Route("Roles")]
        public IActionResult Roles()
        {
            ApiResponse response = new ApiResponse();

            var results = _context.RoleMasters.ToList();
            if (results.Count > 0)
            {
                response.Message = "Records Found!";
                response.Status = "1";
                response.Data = results;
            }
            else
            {
                response.Message = "Records Not Found!";
                response.Status = "0";
            }
            return Ok(response);
        }
        #endregion

        #region User  Roles Managements
        [HttpGet]
        [Route("UserRoles")]
        public IActionResult UserRoles()
        {
            ApiResponse response = new ApiResponse();

            //var results = _context.UserRole.ToList();
            var results = (from ur in _context.UserRoles
                           join u in _context.UserMasters on ur.UserId equals u.Id
                           join r in _context.RoleMasters on ur.RoleId equals r.Id
                           select new
                           {
                               Id = ur.Id,
                               UserId = ur.UserId,
                               RoleId = ur.RoleId,
                               UserName = u.UserName,
                               RoleName = r.Name
                           }
                           ).ToList();
            if (results.Count > 0)
            {
                response.Message = "Records Found!";
                response.Status = "1";
                response.Data = results;
            }
            else
            {
                response.Message = "Records Not Found!";
                response.Status = "0";
            }
            return Ok(response);
        }

        [HttpDelete]
        [Route("UserRole/{id}")]
        public IActionResult UserRoleDelete(int? id)
        {
            ApiResponse response = new ApiResponse();
            var model = _context.UserRoles.Find(id);
            if (model != null)
            {
                try
                {
                    var result = _context.UserRoles.Remove(model);
                    _context.SaveChanges();
                    response.Message = "Record successfully deleted!";
                    response.Status = "1";
                    response.Data = model;
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                    response.Status = "1";
                    response.Data = model;
                    return Ok(response);
                }
            }
            else
            {
                response.Message = "Records Not Found!";
                response.Status = "1";
                return Ok(response);
            }
        }
        #endregion
    }
}
