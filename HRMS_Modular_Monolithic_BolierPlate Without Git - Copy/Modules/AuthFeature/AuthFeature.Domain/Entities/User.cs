// ================================================================
// FILE: Modules/AuthFeature/AuthFeature.Domain/Entities/User.cs
// WHY: Domain entity for User — contains auth credentials + role
// ================================================================
using HRMS.Core.Postgres.Common;

namespace AuthFeature.Domain
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public Role? Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }

    public class Role : BaseEntity
    {
        public string Name { get; set; } = string.Empty;   // SuperAdmin | HR | Manager | Employee
        public string? Description { get; set; }
    }
}
