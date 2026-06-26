
// FILE: Modules/PayrollFeature/PayrollFeature.Application/DTO/PayrollDTOs.cs
using HRMS.Core.Postgres.Common;
using HRMS.Shared.Application.DTOs;
using MediatR;
 
namespace PayrollFeature.Application.DTO
{
    public class GeneratePayrollDto
    {
        public string PayrollPeriodId { get; set; } = string.Empty;
        public string? DepartmentId   { get; set; }  // null = all departments
    }
 
    public class GeneratePayrollRequest : ExecutionRequest, IRequest<BaseResponse<GeneratePayrollResponse>>
    {
        public GeneratePayrollDto? RequestParam { get; set; }
    }
 
    public class GeneratePayrollResponse
    {
        public int ProcessedCount  { get; set; }
        public string PeriodName   { get; set; } = string.Empty;
    }
 
    public class GetPayrollDto
    {
        public string? PayrollPeriodId { get; set; }
        public string? EmployeeId      { get; set; }
        public string? Status          { get; set; }
        public int?    Month           { get; set; }
        public int?    Year            { get; set; }
    }
 
    public class GetPayrollRequest : Request, IRequest<BaseResponsePagination<GetPayrollResponse>>
    {
        public GetPayrollDto? RequestParam { get; set; }
    }
 
    public class PayrollRecordItem
    {
        public string  PayrollRecordId { get; set; } = string.Empty;
        public string  EmployeeId      { get; set; } = string.Empty;
        public string? EmployeeName    { get; set; }
        public string? EmployeeCode    { get; set; }
        public string? DepartmentName  { get; set; }
        public string  PeriodName      { get; set; } = string.Empty;
        public decimal BasicSalary     { get; set; }
        public decimal HRA             { get; set; }
        public decimal DA              { get; set; }
        public decimal TA              { get; set; }
        public decimal GrossSalary     { get; set; }
        public decimal PF              { get; set; }
        public decimal TDS             { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary       { get; set; }
        public int     PaidDays        { get; set; }
        public int     WorkingDays     { get; set; }
        public string  Status          { get; set; } = string.Empty;
        public DateTime? PaymentDate   { get; set; }
    }
 
    public class GetPayrollResponse    { public List<PayrollRecordItem>? Records { get; set; } }
 
    // Payroll calculation service interface
    public interface IPayrollCalculationService
    {
        Task<PayrollFeature.Domain.PayrollRecord> CalculatePayrollAsync(
            string employeeId, string periodId, decimal basicSalary, int workingDays, int paidDays, int leaveDays, int absentDays);
    }
 
    public class PayrollCalculationService : IPayrollCalculationService
    {
        public Task<PayrollFeature.Domain.PayrollRecord> CalculatePayrollAsync(
            string employeeId, string periodId, decimal basicSalary,
            int workingDays, int paidDays, int leaveDays, int absentDays)
        {
            // Standard Indian payroll calculation
            var hra    = Math.Round(basicSalary * 0.40m, 2);  // 40% HRA
            var da     = Math.Round(basicSalary * 0.10m, 2);  // 10% DA
            var ta     = Math.Round(basicSalary * 0.05m, 2);  //  5% TA
            var gross  = basicSalary + hra + da + ta;
 
            // Deductions
            var pf     = Math.Round(basicSalary * 0.12m, 2);  // 12% PF
            var esi    = gross > 21000 ? 0 : Math.Round(gross * 0.0075m, 2); // ESI if gross <= 21000
            var tds    = CalculateTDS(gross * 12) / 12;  // Monthly TDS
            var totalDeductions = pf + esi + tds;
            var net    = gross - totalDeductions;
 
            // Pro-rate if absent
            if (workingDays > 0 && paidDays < workingDays)
            {
                var ratio = (decimal)paidDays / workingDays;
                gross = Math.Round(gross * ratio, 2);
                totalDeductions = Math.Round(totalDeductions * ratio, 2);
                net   = gross - totalDeductions;
            }
 
            return Task.FromResult(new PayrollFeature.Domain.PayrollRecord
            {
                EmployeeId      = employeeId,
                PayrollPeriodId = periodId,
                BasicSalary     = basicSalary,
                HRA             = hra,
                DA              = da,
                TA              = ta,
                GrossSalary     = gross,
                PF              = pf,
                ESI             = esi,
                TDS             = Math.Round(tds, 2),
                TotalDeductions = totalDeductions,
                NetSalary       = Math.Round(net, 2),
                WorkingDays     = workingDays,
                PaidDays        = paidDays,
                LeaveDays       = leaveDays,
                AbsentDays      = absentDays,
                Status          = "Draft"
            });
        }
 
        private static decimal CalculateTDS(decimal annualGross)
        {
            // Simplified Indian income tax slab (new regime 2024-25)
            if (annualGross <= 300000) return 0;
            if (annualGross <= 600000) return (annualGross - 300000) * 0.05m;
            if (annualGross <= 900000) return 15000 + (annualGross - 600000) * 0.10m;
            if (annualGross <= 1200000) return 45000 + (annualGross - 900000) * 0.15m;
            if (annualGross <= 1500000) return 90000 + (annualGross - 1200000) * 0.20m;
            return 150000 + (annualGross - 1500000) * 0.30m;
        }
    }
}
 