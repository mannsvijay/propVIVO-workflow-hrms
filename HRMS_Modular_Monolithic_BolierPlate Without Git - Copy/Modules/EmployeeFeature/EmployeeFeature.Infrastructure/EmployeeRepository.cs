
// ================================================================
// FILE: Modules/EmployeeFeature/EmployeeFeature.Infrastructure/EmployeeRepository.cs
// ================================================================
using HRMS.Core.Postgres.Data;
using HRMS.Core.Postgres.Helper;
using HRMS.Core.Postgres.Interfaces;
using HRMS.Core.Postgres.Repositories;
using HRMS.Core.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using EmployeeFeature.Application.DTO;
using EmployeeFeature.Application.Repository;
using EmployeeFeature.Domain;
 
namespace EmployeeFeature.Infrastructure
{
    public class EmployeeEntityConfigurator : IPostgresEntityConfigurator
    {
        public void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(128);
                entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(128);
                entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.BasicSalary).HasColumnType("numeric(12,2)");
                entity.HasIndex(e => e.EmployeeCode).IsUnique();
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.DepartmentId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.DocumentType);
                // Ignore navigation properties that aren't real columns
                entity.Ignore(e => e.FullName);
            });
        }
    }
 
    public class EmployeeRepository : PostgresDbRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(
            PostgresDbContext context, ILogger<EmployeeRepository> logger,
            ITelemetryService telemetryService, IHttpContextAccessor httpContextAccessor)
            : base(context, logger, telemetryService, httpContextAccessor) { }
 
        public override string TableName => nameof(Employee);
        public override string GenerateId(Employee entity) => Guid.NewGuid().ToString();
 
        private Expression<Func<Employee, bool>> BuildQuery(GetAllEmployeesRequest request)
        {
            Expression<Func<Employee, bool>> filter = x => x.DocumentType == nameof(Employee);
 
            if (request.RequestParam == null) return filter;
            var p = request.RequestParam;
 
            if (!string.IsNullOrEmpty(p.EmployeeId))
                filter = filter.And(x => x.Id == p.EmployeeId);
            if (!string.IsNullOrEmpty(p.DepartmentId))
                filter = filter.And(x => x.DepartmentId == p.DepartmentId);
            if (!string.IsNullOrEmpty(p.ManagerId))
                filter = filter.And(x => x.ManagerId == p.ManagerId);
            if (!string.IsNullOrEmpty(p.EmploymentType))
                filter = filter.And(x => x.EmploymentType == p.EmploymentType);
            if (p.IsActive.HasValue)
                filter = filter.And(x => x.IsActive == p.IsActive.Value);
 
            if (!string.IsNullOrEmpty(p.Keyword))
            {
                var kw = p.Keyword.ToLower().Trim();
                filter = filter.And(x =>
                    (x.FirstName != null && x.FirstName.ToLower().Contains(kw)) ||
                    (x.LastName  != null && x.LastName.ToLower().Contains(kw))  ||
                    (x.Email     != null && x.Email.ToLower().Contains(kw))      ||
                    (x.EmployeeCode != null && x.EmployeeCode.ToLower().Contains(kw)) ||
                    (x.Designation  != null && x.Designation.ToLower().Contains(kw))
                );
            }
 
            return filter;
        }
 
        public async Task<(IEnumerable<Employee> result, int count)> GetAllEmployeesWithCountAsync(GetAllEmployeesRequest request)
        {
            var orderBy = request.OrderByCriteria != null ? OrderBy(request) : (Expression<Func<Employee, object>>?)null;
            var defaultOrder = orderBy ?? (x => x.JoiningDate);
            return await GetItemsWithCountAsync(BuildQuery(request), request, defaultOrder);
        }
 
        public async Task<Employee?> GetEmployeeAsync(GetAllEmployeesRequest request)
            => await GetItemAsync(BuildQuery(request));
 
        public async Task<string> GenerateEmployeeCodeAsync()
        {
            var lastEmployee = await _dbSet
                .AsNoTracking()
                .Where(e => e.DocumentType == nameof(Employee))
                .OrderByDescending(e => e.EmployeeCode)
                .FirstOrDefaultAsync();
 
            if (lastEmployee == null) return "EMP-0001";
 
            var parts  = lastEmployee.EmployeeCode.Split('-');
            var lastNum = int.TryParse(parts.LastOrDefault(), out var n) ? n : 0;
            return $"EMP-{(lastNum + 1):D4}";
        }
    }
}
 