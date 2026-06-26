// FILE: Modules/AttendanceFeature/AttendanceFeature.Domain/Attendance.cs
namespace AttendanceFeature.Domain
{
    public class Attendance : HRMS.Core.Postgres.Common.BaseEntity
    {
        public string  EmployeeId     { get; set; } = string.Empty;
        public DateTime Date          { get; set; }
        public DateTime? CheckInTime  { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string  Status         { get; set; } = "Present";
        // Present | Absent | HalfDay | WFH | OnLeave | Holiday
        public decimal? WorkingHours  { get; set; }
        public string?  Notes         { get; set; }
        public bool IsManualEntry     { get; set; } = false;
 
        // Denormalized for read queries
        public string? EmployeeName   { get; set; }
        public string? EmployeeCode   { get; set; }
    }
}