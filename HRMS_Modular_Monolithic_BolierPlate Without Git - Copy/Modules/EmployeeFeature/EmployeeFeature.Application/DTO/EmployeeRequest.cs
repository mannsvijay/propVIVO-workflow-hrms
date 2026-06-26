
// ================================================================
// FILE: Modules/EmployeeFeature/EmployeeFeature.Application/DTO/EmployeeRequest.cs
// ================================================================
using HRMS.Core.Postgres.Common;
using HRMS.Shared.Application.DTOs;
using MediatR;
 
namespace EmployeeFeature.Application.DTO
{
    public class CreateEmployeeDto
    {
        public string? DepartmentId    { get; set; }
        public string? ManagerId       { get; set; }
        public string  FirstName       { get; set; } = string.Empty;
        public string  LastName        { get; set; } = string.Empty;
        public string  Email           { get; set; } = string.Empty;
        public string? Phone           { get; set; }
        public DateTime? DateOfBirth   { get; set; }
        public string? Gender          { get; set; }
        public string? Designation     { get; set; }
        public string  EmploymentType  { get; set; } = "FullTime";
        public DateTime JoiningDate    { get; set; } = DateTime.UtcNow;
        public string? Address         { get; set; }
        public string? City            { get; set; }
        public string? State           { get; set; }
        public string? Country         { get; set; } = "India";
        public string? PinCode         { get; set; }
        public decimal BasicSalary     { get; set; }
    }
 
    public class CreateEmployeeRequest : ExecutionRequest, IRequest<BaseResponse<CreateEmployeeResponse>>
    {
        public CreateEmployeeDto? RequestParam { get; set; }
    }
 
    public class UpdateEmployeeDto : CreateEmployeeDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? LeavingDate { get; set; }
    }
 
    public class UpdateEmployeeRequest : ExecutionRequest, IRequest<BaseResponse<UpdateEmployeeResponse>>
    {
        public UpdateEmployeeDto? RequestParam { get; set; }
    }
 
    public class DeleteEmployeeDto
    {
        public string EmployeeId { get; set; } = string.Empty;
    }
 
    public class DeleteEmployeeRequest : ExecutionRequest, IRequest<BaseResponse<DeleteEmployeeResponse>>
    {
        public DeleteEmployeeDto? RequestParam { get; set; }
    }
 
    public class GetAllEmployeesDto
    {
        public string? EmployeeId      { get; set; }
        public string? DepartmentId    { get; set; }
        public string? ManagerId       { get; set; }
        public string? EmploymentType  { get; set; }
        public bool?   IsActive        { get; set; } = true;
        public string? Keyword         { get; set; }
    }
 
    public class GetAllEmployeesRequest : Request, IRequest<BaseResponsePagination<GetAllEmployeesResponse>>
    {
        public GetAllEmployeesDto? RequestParam { get; set; }
    }
}