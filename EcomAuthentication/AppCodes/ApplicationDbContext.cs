using EcomAuthentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcomAuthentication.AppCodes
{
    public partial class ApplicationDbContext : IdentityDbContext<ApiUser, ApiRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<RoleMaster> RoleMasters { get; set; } = default!;
        public DbSet<UserRole> UserRoles { get; set; } = default!;
        public virtual DbSet<UserMaster> UserMasters { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApiUser>(entity =>
            {
                entity.ToTable(name: "UserMaster");
                //entity.Property("UserName").IsRequired(true);
                //entity.Property("Email").IsRequired(true);
                //entity.Property("UserFulName").HasMaxLength(200);
            });

            builder.Entity<ApiRole>(entity =>
            {
                entity.ToTable(name: "RoleMaster");
                //entity.Property("Name").IsRequired(true);
                //entity.Property("Description").HasMaxLength(200);
            });

            builder.Entity<IdentityUserClaim<int>>(entity =>
            {
                entity.ToTable("UserClaim");
            });

            builder.Entity<IdentityUserLogin<int>>(entity =>
            {
                entity.ToTable("UserLogin");
            });

            builder.Entity<IdentityRoleClaim<int>>(entity =>
            {
                entity.ToTable("RoleClaim");
            });

            builder.Entity<IdentityUserRole<int>>(entity =>
            {
                entity.ToTable("UserRole");
            });

            builder.Entity<IdentityUserToken<int>>(entity =>
            {
                entity.ToTable("UserToken");
            });
        }
    }
}
