// ================================================================
// FILE: Modules/EmployeeFeature/EmployeeFeature.Domain/Employee.cs
// WHY: Domain entity — heart of HRMS. Every other module links to Employee.
// ================================================================
using HRMS.Core.Postgres.Common;
using HRMS.Shared.Domain.Entity;
 
namespace EmployeeFeature.Domain
{
    public class Employee : BaseEntity
    {
        public string EmployeeCode { get; set; } = string.Empty;   // EMP-0001
        public string? UserId       { get; set; }
        public string DepartmentId  { get; set; } = string.Empty;
        public string? ManagerId    { get; set; }
 
        public string FirstName     { get; set; } = string.Empty;
        public string LastName      { get; set; } = string.Empty;
        public string Email         { get; set; } = string.Empty;
        public string? Phone        { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender        { get; set; }
        public string? Designation   { get; set; }
        public string EmploymentType { get; set; } = "FullTime";
 
        public DateTime JoiningDate  { get; set; }
        public DateTime? LeavingDate { get; set; }
        public bool IsActive         { get; set; } = true;
 
        public string? Address { get; set; }
        public string? City    { get; set; }
        public string? State   { get; set; }
        public string? Country { get; set; } = "India";
        public string? PinCode { get; set; }
 
        public decimal BasicSalary   { get; set; }
 
        // Navigations (loaded by joins — not OwnsOne)
        public string? DepartmentName { get; set; }  // denormalized for queries
        public string? ManagerName    { get; set; }  // denormalized for queries
 
        public string FullName => $"{FirstName} {LastName}";
    }
}
 
 