
// FILE: Modules/LeaveFeature/LeaveFeature.Application/DTO/LeaveRequestDTOs.cs
using HRMS.Core.Postgres.Common;
using HRMS.Shared.Application.DTOs;
using MediatR;
 
namespace LeaveFeature.Application.DTO
{
    public class CreateLeaveRequestDto
    {
        public string   EmployeeId  { get; set; } = string.Empty;
        public string   LeaveTypeId { get; set; } = string.Empty;
        public DateTime StartDate   { get; set; }
        public DateTime EndDate     { get; set; }
        public string?  Reason      { get; set; }
    }
 
    public class CreateLeaveRequest : ExecutionRequest, IRequest<BaseResponse<LeaveActionResponse>>
    {
        public CreateLeaveRequestDto? RequestParam { get; set; }
    }
 
    public class ApproveRejectLeaveDto
    {
        public string  LeaveRequestId { get; set; } = string.Empty;
        public string  Action         { get; set; } = string.Empty;  // Approved | Rejected
        public string  ReviewedBy     { get; set; } = string.Empty;
        public string? ReviewComments { get; set; }
    }
 
    public class ApproveRejectLeaveRequest : ExecutionRequest, IRequest<BaseResponse<LeaveActionResponse>>
    {
        public ApproveRejectLeaveDto? RequestParam { get; set; }
    }
 
    public class GetLeaveRequestsDto
    {
        public string? LeaveRequestId { get; set; }
        public string? EmployeeId     { get; set; }
        public string? LeaveTypeId    { get; set; }
        public string? Status         { get; set; }
        public int?    Year           { get; set; }
    }
 
    public class GetLeaveRequestsRequest : Request, IRequest<BaseResponsePagination<GetLeaveRequestsResponse>>
    {
        public GetLeaveRequestsDto? RequestParam { get; set; }
    }
 
    public class LeaveActionResponse { public string? LeaveRequestId { get; set; } }
 
    public class LeaveRequestItem
    {
        public string    LeaveRequestId { get; set; } = string.Empty;
        public string    EmployeeId     { get; set; } = string.Empty;
        public string?   EmployeeName   { get; set; }
        public string    LeaveTypeId    { get; set; } = string.Empty;
        public string?   LeaveTypeName  { get; set; }
        public DateTime  StartDate      { get; set; }
        public DateTime  EndDate        { get; set; }
        public decimal   TotalDays      { get; set; }
        public string?   Reason         { get; set; }
        public string    Status         { get; set; } = string.Empty;
        public string?   ReviewedBy     { get; set; }
        public DateTime? ReviewedAt     { get; set; }
        public string?   ReviewComments { get; set; }
        public DateTime? CreatedOn      { get; set; }
    }
 
    public class GetLeaveRequestsResponse { public List<LeaveRequestItem>? LeaveRequests { get; set; } }
}