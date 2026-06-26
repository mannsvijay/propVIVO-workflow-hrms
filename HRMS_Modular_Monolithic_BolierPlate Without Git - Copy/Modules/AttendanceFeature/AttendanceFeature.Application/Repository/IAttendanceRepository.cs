
// FILE: Modules/AttendanceFeature/AttendanceFeature.Application/Repository/IAttendanceRepository.cs
namespace AttendanceFeature.Application.Repository
{
    public interface IAttendanceRepository : HRMS.Core.Postgres.Repositories.IPostgresRepository<AttendanceFeature.Domain.Attendance>
    {
        Task<(IEnumerable<AttendanceFeature.Domain.Attendance> result, int count)> GetAllAttendanceWithCountAsync(AttendanceFeature.Application.DTO.GetAttendanceRequest request);
        Task<AttendanceFeature.Domain.Attendance?> GetAttendanceByEmployeeAndDateAsync(string employeeId, DateTime date);
    }
}
 
 
// ================================================================
// LEAVE FEATURE — Complete Module
// ================================================================
 
namespace LeaveFeature.Domain
{
    // FILE: Modules/LeaveFeature/LeaveFeature.Domain/LeaveRequest.cs
    public class LeaveRequestEntity : HRMS.Core.Postgres.Common.BaseEntity
    {
        public string   EmployeeId     { get; set; } = string.Empty;
        public string   LeaveTypeId    { get; set; } = string.Empty;
        public DateTime StartDate      { get; set; }
        public DateTime EndDate        { get; set; }
        public decimal  TotalDays      { get; set; }
        public string?  Reason         { get; set; }
        public string   Status         { get; set; } = "Pending";
        // Pending | Approved | Rejected | Cancelled
        public string?  ReviewedBy     { get; set; }
        public DateTime? ReviewedAt    { get; set; }
        public string?  ReviewComments { get; set; }
 
        // Denormalized
        public string? EmployeeName  { get; set; }
        public string? LeaveTypeName { get; set; }
    }
}