
// FILE: Modules/AttendanceFeature/AttendanceFeature.Application/DTO/AttendanceHandler.cs
using AttendanceFeature.Application.Repository;
using AttendanceFeature.Domain;
using HRMS.Core.Telemetry.Exceptions;
using HRMS.Shared.Application.Constants;
using HRMS.Shared.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
 
namespace AttendanceFeature.Application.DTO
{
    public class CheckInHandler : IRequestHandler<CheckInRequest, BaseResponse<AttendanceActionResponse>>
    {
        private readonly IAttendanceRepository _repo;
        public CheckInHandler(IAttendanceRepository repo) { _repo = repo; }
 
        public async Task<BaseResponse<AttendanceActionResponse>> Handle(CheckInRequest request, CancellationToken ct)
        {
            if (request?.RequestParam == null) throw new BadRequestException(Messaging.InvalidRequest);
 
            var today = DateTime.UtcNow.Date;
            var existing = await _repo.GetAttendanceByEmployeeAndDateAsync(request.RequestParam.EmployeeId, today);
 
            if (existing != null)
                throw new BadRequestException("Already checked in today");
 
            var attendance = new Attendance
            {
                EmployeeId    = request.RequestParam.EmployeeId,
                Date          = today,
                CheckInTime   = DateTime.UtcNow,
                Status        = "Present",
                Notes         = request.RequestParam.Notes,
                IsManualEntry = false
            };
 
            attendance = await _repo.AddItemAsync(attendance);
 
            return new BaseResponse<AttendanceActionResponse>
            {
                Data = new AttendanceActionResponse { AttendanceId = attendance.Id },
                StatusCode = StatusCodes.Status200OK,
                Message = "Checked in successfully",
                Success = true
            };
        }
    }
 
    public class CheckOutHandler : IRequestHandler<CheckOutRequest, BaseResponse<AttendanceActionResponse>>
    {
        private readonly IAttendanceRepository _repo;
        public CheckOutHandler(IAttendanceRepository repo) { _repo = repo; }
 
        public async Task<BaseResponse<AttendanceActionResponse>> Handle(CheckOutRequest request, CancellationToken ct)
        {
            if (request?.RequestParam == null) throw new BadRequestException(Messaging.InvalidRequest);
 
            var attendance = await _repo.GetItemAsync(a => a.Id == request.RequestParam.AttendanceId)
                ?? throw new NotFoundException("Attendance record not found");
 
            if (attendance.CheckOutTime != null)
                throw new BadRequestException("Already checked out");
 
            var checkOut = DateTime.UtcNow;
            var working  = attendance.CheckInTime.HasValue
                ? (decimal)(checkOut - attendance.CheckInTime.Value).TotalHours
                : 0;
 
            attendance.CheckOutTime = checkOut;
            attendance.WorkingHours = Math.Round(working, 2);
            attendance.Status       = working >= 4 && working < 7 ? "HalfDay" : "Present";
            if (!string.IsNullOrEmpty(request.RequestParam.Notes)) attendance.Notes = request.RequestParam.Notes;
 
            await _repo.UpdateItemAsync(attendance.Id, attendance);
 
            return new BaseResponse<AttendanceActionResponse>
            {
                Data = new AttendanceActionResponse { AttendanceId = attendance.Id },
                StatusCode = StatusCodes.Status200OK,
                Message = "Checked out successfully",
                Success = true
            };
        }
    }
 
    public class GetAttendanceHandler : IRequestHandler<GetAttendanceRequest, BaseResponsePagination<GetAttendanceResponse>>
    {
        private readonly IAttendanceRepository _repo;
        public GetAttendanceHandler(IAttendanceRepository repo) { _repo = repo; }
 
        public async Task<BaseResponsePagination<GetAttendanceResponse>> Handle(GetAttendanceRequest request, CancellationToken ct)
        {
            if (request == null) throw new BadRequestException(Messaging.InvalidRequest);
            var (items, count) = await _repo.GetAllAttendanceWithCountAsync(request);
 
            var response = new BaseResponsePagination<GetAttendanceResponse> { Success = true, StatusCode = StatusCodes.Status200OK };
            if (items?.Any() == true)
            {
                response.Data = new GetAttendanceResponse
                {
                    Attendance = items.Select(a => new AttendanceItem
                    {
                        AttendanceId  = a.Id,
                        EmployeeId    = a.EmployeeId,
                        Date          = a.Date,
                        CheckInTime   = a.CheckInTime,
                        CheckOutTime  = a.CheckOutTime,
                        Status        = a.Status,
                        WorkingHours  = a.WorkingHours,
                        Notes         = a.Notes,
                        IsManualEntry = a.IsManualEntry
                    }).ToList()
                };
 
                if (request.PageCriteria?.EnablePage == true)
                    response.Meta = new Meta { Skip = request.PageCriteria.Skip, Take = request.PageCriteria.PageSize, TotalCount = count };
            }
 
            return response;
        }
    }
}