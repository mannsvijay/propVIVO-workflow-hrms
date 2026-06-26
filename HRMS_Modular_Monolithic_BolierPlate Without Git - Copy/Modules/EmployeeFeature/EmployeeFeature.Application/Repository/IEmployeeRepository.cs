 
// ================================================================
// FILE: Modules/EmployeeFeature/EmployeeFeature.Application/Repository/IEmployeeRepository.cs
// ================================================================
using EmployeeFeature.Domain;
using HRMS.Core.Postgres.Repositories;
 
namespace EmployeeFeature.Application.Repository
{
    public interface IEmployeeRepository : IPostgresRepository<Employee>
    {
        Task<(IEnumerable<Employee> result, int count)> GetAllEmployeesWithCountAsync(EmployeeFeature.Application.DTO.GetAllEmployeesRequest request);
        Task<Employee?> GetEmployeeAsync(EmployeeFeature.Application.DTO.GetAllEmployeesRequest request);
        Task<string> GenerateEmployeeCodeAsync();
    }
}