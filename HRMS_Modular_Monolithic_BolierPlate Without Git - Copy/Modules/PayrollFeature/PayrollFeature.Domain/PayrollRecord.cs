
// ================================================================
// PAYROLL FEATURE — Complete Module
// ================================================================
 
namespace PayrollFeature.Domain
{
    // FILE: Modules/PayrollFeature/PayrollFeature.Domain/PayrollRecord.cs
    public class PayrollRecord : HRMS.Core.Postgres.Common.BaseEntity
    {
        public string  PayrollPeriodId  { get; set; } = string.Empty;
        public string  EmployeeId       { get; set; } = string.Empty;
        public decimal BasicSalary      { get; set; }
        public decimal HRA              { get; set; }  // 40% of basic
        public decimal DA               { get; set; }  // 10% of basic
        public decimal TA               { get; set; }  //  5% of basic
        public decimal OtherAllowances  { get; set; }
        public decimal GrossSalary      { get; set; }
        public decimal PF               { get; set; }  // 12% of basic
        public decimal ESI              { get; set; }
        public decimal TDS              { get; set; }
        public decimal OtherDeductions  { get; set; }
        public decimal TotalDeductions  { get; set; }
        public decimal NetSalary        { get; set; }
        public int     WorkingDays      { get; set; }
        public int     PaidDays         { get; set; }
        public int     LeaveDays        { get; set; }
        public int     AbsentDays       { get; set; }
        public string  Status           { get; set; } = "Draft";
        public DateTime? PaymentDate    { get; set; }
        public string? PaymentMethod    { get; set; }
        public string? BankAccount      { get; set; }
 
        // Denormalized
        public string? EmployeeName     { get; set; }
        public string? EmployeeCode     { get; set; }
        public string? DepartmentName   { get; set; }
    }
 
    public class PayrollPeriod : HRMS.Core.Postgres.Common.BaseEntity
    {
        public string  Name       { get; set; } = string.Empty;
        public int     Month      { get; set; }
        public int     Year       { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate   { get; set; }
        public string  Status     { get; set; } = "Draft";
        public DateTime? ProcessedAt { get; set; }
        public DateTime? PaidAt      { get; set; }
    }
}