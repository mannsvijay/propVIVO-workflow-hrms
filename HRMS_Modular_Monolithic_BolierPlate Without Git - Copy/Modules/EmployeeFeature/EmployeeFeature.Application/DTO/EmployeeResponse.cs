// ================================================================
// FILE: Modules/EmployeeFeature/EmployeeFeature.Application/DTO/EmployeeResponse.cs
// ================================================================
using HRMS.Shared.Application.DTOs;
 
namespace EmployeeFeature.Application.DTO
{
    public class CreateEmployeeResponse  { public string? EmployeeId { get; set; } }
    public class UpdateEmployeeResponse  { public string? EmployeeId { get; set; } }
    public class DeleteEmployeeResponse  { public string? EmployeeId { get; set; } }
 
    public class EmployeeItem
    {
        public string  EmployeeId     { get; set; } = string.Empty;
        public string  EmployeeCode   { get; set; } = string.Empty;
        public string  FirstName      { get; set; } = string.Empty;
        public string  LastName       { get; set; } = string.Empty;
        public string  FullName       { get; set; } = string.Empty;
        public string  Email          { get; set; } = string.Empty;
        public string? Phone          { get; set; }
        public string? Gender         { get; set; }
        public string? Designation    { get; set; }
        public string  EmploymentType { get; set; } = string.Empty;
        public string  DepartmentId   { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? ManagerId      { get; set; }
        public string? ManagerName    { get; set; }
        public DateTime JoiningDate   { get; set; }
        public DateTime? LeavingDate  { get; set; }
        public bool IsActive          { get; set; }
        public decimal BasicSalary    { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime? CreatedOn    { get; set; }
    }
 
    public class GetAllEmployeesResponse
    {
        public List<EmployeeItem>? Employees { get; set; }
    }
}
 
 