// ================================================================
    // FILE: Modules/AuthFeature/AuthFeature.Infrastructure/AuthRepository.cs
    // WHY: EF Core implementation — follows exact same pattern as TodoRepository
    // ================================================================
    public class UserEntityConfigurator : HRMS.Core.Postgres.Interfaces.IPostgresEntityConfigurator
    {
        public void Configure(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthFeature.Domain.User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(128);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(512);
                entity.Property(e => e.RoleId).IsRequired().HasMaxLength(128);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.DocumentType);
                entity.HasIndex(e => e.RoleId);
                entity.HasOne(e => e.Role)
                    .WithMany()
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);
            });
 
            modelBuilder.Entity<AuthFeature.Domain.Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(128);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
            });
        }
    }
 
    public class AuthRepository : HRMS.Core.Postgres.Repositories.PostgresDbRepository<AuthFeature.Domain.User>,
        AuthFeature.Application.Repository.IAuthRepository
    {
        private readonly HRMS.Core.Postgres.Data.PostgresDbContext _ctx;
 
        public AuthRepository(
            HRMS.Core.Postgres.Data.PostgresDbContext context,
            Microsoft.Extensions.Logging.ILogger<AuthRepository> logger,
            HRMS.Core.Telemetry.ITelemetryService telemetryService,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(context, logger, telemetryService, httpContextAccessor)
        {
            _ctx = context;
        }
 
        public override string TableName => "Users";
        public override string GenerateId(AuthFeature.Domain.User entity) => Guid.NewGuid().ToString();
 
        public async Task<AuthFeature.Domain.User?> GetByEmailAsync(string email)
            => await _ctx.Set<AuthFeature.Domain.User>()
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.DocumentType == "User");
 
        public async Task<AuthFeature.Domain.User?> GetByRefreshTokenAsync(string refreshToken)
            => await _ctx.Set<AuthFeature.Domain.User>()
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.DocumentType == "User");
 
        public async Task UpdateRefreshTokenAsync(string userId, string? refreshToken, DateTime? expiry)
        {
            var user = await _ctx.Set<AuthFeature.Domain.User>().FindAsync(userId);
            if (user == null) return;
            user.RefreshToken       = refreshToken;
            user.RefreshTokenExpiry = expiry;
            user.ModifiedOn         = DateTime.UtcNow;
            await _ctx.SaveChangesAsync();
        }
 
        public async Task UpdateLastLoginAsync(string userId)
        {
            var user = await _ctx.Set<AuthFeature.Domain.User>().FindAsync(userId);
            if (user == null) return;
            user.LastLoginAt = DateTime.UtcNow;
            user.ModifiedOn  = DateTime.UtcNow;
            await _ctx.SaveChangesAsync();
        }
 
        public async Task<AuthFeature.Domain.Role?> GetRoleByIdAsync(string roleId)
            => await _ctx.Set<AuthFeature.Domain.Role>().AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId);
 
        public async Task<IEnumerable<AuthFeature.Domain.Role>> GetAllRolesAsync()
            => await _ctx.Set<AuthFeature.Domain.Role>().AsNoTracking().ToListAsync();
    }
}
 