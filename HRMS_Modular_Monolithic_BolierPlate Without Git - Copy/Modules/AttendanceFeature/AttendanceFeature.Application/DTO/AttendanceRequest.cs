using HRMS.Core.Postgres.Common;
using HRMS.Shared.Application.DTOs;
using MediatR;
 
namespace AttendanceFeature.Application.DTO
{
    public class CheckInDto
    {
        public string  EmployeeId { get; set; } = string.Empty;
        public string? Notes      { get; set; }
    }
 
    public class CheckInRequest : ExecutionRequest, IRequest<BaseResponse<AttendanceActionResponse>>
    {
        public CheckInDto? RequestParam { get; set; }
    }
 
    public class CheckOutDto
    {
        public string  AttendanceId { get; set; } = string.Empty;
        public string? Notes        { get; set; }
    }
 
    public class CheckOutRequest : ExecutionRequest, IRequest<BaseResponse<AttendanceActionResponse>>
    {
        public CheckOutDto? RequestParam { get; set; }
    }
 
    public class MarkAttendanceDto
    {
        public string   EmployeeId { get; set; } = string.Empty;
        public DateTime Date       { get; set; }
        public string   Status     { get; set; } = "Present";
        public string?  Notes      { get; set; }
        public bool     IsManualEntry { get; set; } = true;
    }
 
    public class MarkAttendanceRequest : ExecutionRequest, IRequest<BaseResponse<AttendanceActionResponse>>
    {
        public MarkAttendanceDto? RequestParam { get; set; }
    }
 
    public class GetAttendanceDto
    {
        public string?   AttendanceId { get; set; }
        public string?   EmployeeId   { get; set; }
        public string?   Status       { get; set; }
        public DateTime? DateFrom     { get; set; }
        public DateTime? DateTo       { get; set; }
    }
 
    public class GetAttendanceRequest : Request, IRequest<BaseResponsePagination<GetAttendanceResponse>>
    {
        public GetAttendanceDto? RequestParam { get; set; }
    }
 
    public class AttendanceActionResponse { public string? AttendanceId { get; set; } }
 
    public class AttendanceItem
    {
        public string    AttendanceId  { get; set; } = string.Empty;
        public string    EmployeeId    { get; set; } = string.Empty;
        public string?   EmployeeName  { get; set; }
        public string?   EmployeeCode  { get; set; }
        public DateTime  Date          { get; set; }
        public DateTime? CheckInTime   { get; set; }
        public DateTime? CheckOutTime  { get; set; }
        public string    Status        { get; set; } = string.Empty;
        public decimal?  WorkingHours  { get; set; }
        public string?   Notes         { get; set; }
        public bool      IsManualEntry { get; set; }
    }
 
    public class GetAttendanceResponse { public List<AttendanceItem>? Attendance { get; set; } }
}
 