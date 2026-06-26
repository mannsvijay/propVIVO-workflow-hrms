
// ================================================================
// FILE: Modules/EmployeeFeature/EmployeeFeature.Application/DTO/EmployeeHandler.cs
// WHY: MediatR handlers — same structure as TodoHandler, easy to follow
// ================================================================
using AutoMapper;
using EmployeeFeature.Application.Repository;
using EmployeeFeature.Domain;
using HRMS.Core.Telemetry.Exceptions;
using HRMS.Shared.Application.Constants;
using HRMS.Shared.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
 
namespace EmployeeFeature.Application.DTO
{
    public class CreateEmployeeHandler : IRequestHandler<CreateEmployeeRequest, BaseResponse<CreateEmployeeResponse>>
    {
        private readonly IMapper _mapper;
        private readonly IEmployeeRepository _repo;
 
        public CreateEmployeeHandler(IMapper mapper, IEmployeeRepository repo)
        {
            _mapper = mapper; _repo = repo;
        }
 
        public async Task<BaseResponse<CreateEmployeeResponse>> Handle(CreateEmployeeRequest request, CancellationToken ct)
        {
            if (request?.RequestParam == null)
                throw new BadRequestException(Messaging.InvalidRequest);
 
            var employee = _mapper.Map<Employee>(request.RequestParam);
            employee.EmployeeCode = await _repo.GenerateEmployeeCodeAsync();
            employee = await _repo.AddItemAsync(employee);
 
            return new BaseResponse<CreateEmployeeResponse>
            {
                Data       = new CreateEmployeeResponse { EmployeeId = employee.Id },
                StatusCode = StatusCodes.Status200OK,
                Message    = string.Format(Messaging.Insert, nameof(Employee)),
                Success    = true
            };
        }
    }
 
    public sealed class GetAllEmployeesHandler : IRequestHandler<GetAllEmployeesRequest, BaseResponsePagination<GetAllEmployeesResponse>>
    {
        private readonly IMapper _mapper;
        private readonly IEmployeeRepository _repo;
 
        public GetAllEmployeesHandler(IEmployeeRepository repo, IMapper mapper) { _repo = repo; _mapper = mapper; }
 
        public async Task<BaseResponsePagination<GetAllEmployeesResponse>> Handle(GetAllEmployeesRequest request, CancellationToken ct)
        {
            if (request == null) throw new BadRequestException(Messaging.InvalidRequest);
 
            var (employees, count) = await _repo.GetAllEmployeesWithCountAsync(request);
            var response = new BaseResponsePagination<GetAllEmployeesResponse> { Success = true, StatusCode = StatusCodes.Status200OK };
 
            if (employees?.Any() == true)
            {
                response.Data = new GetAllEmployeesResponse
                {
                    Employees = _mapper.Map<List<EmployeeItem>>(employees.ToList())
                };
 
                if (request.PageCriteria?.EnablePage == true)
                    response.Meta = new Meta { Skip = request.PageCriteria.Skip, Take = request.PageCriteria.PageSize, TotalCount = count };
            }
 
            return response;
        }
    }
 
    public sealed class UpdateEmployeeHandler : IRequestHandler<UpdateEmployeeRequest, BaseResponse<UpdateEmployeeResponse>>
    {
        private readonly IMapper _mapper;
        private readonly IEmployeeRepository _repo;
 
        public UpdateEmployeeHandler(IMapper mapper, IEmployeeRepository repo) { _mapper = mapper; _repo = repo; }
 
        public async Task<BaseResponse<UpdateEmployeeResponse>> Handle(UpdateEmployeeRequest request, CancellationToken ct)
        {
            if (request?.RequestParam == null) throw new BadRequestException(Messaging.InvalidRequest);
 
            var existing = await _repo.GetEmployeeAsync(new GetAllEmployeesRequest
            {
                RequestParam = new GetAllEmployeesDto { EmployeeId = request.RequestParam.EmployeeId }
            }) ?? throw new NotFoundException(string.Format(Messaging.NotFound, nameof(Employee)));
 
            var updated = _mapper.Map<Employee>(request.RequestParam);
            updated.CreatedOn          = existing.CreatedOn;
            updated.CreatedByUserId    = existing.CreatedByUserId;
            updated.CreatedByUserName  = existing.CreatedByUserName;
            updated.EmployeeCode       = existing.EmployeeCode;
 
            await _repo.UpdateItemAsync(existing.Id, updated);
 
            return new BaseResponse<UpdateEmployeeResponse>
            {
                Data = new UpdateEmployeeResponse { EmployeeId = existing.Id },
                StatusCode = StatusCodes.Status200OK,
                Message = string.Format(Messaging.Update, nameof(Employee)),
                Success = true
            };
        }
    }
 
    public sealed class DeleteEmployeeHandler : IRequestHandler<DeleteEmployeeRequest, BaseResponse<DeleteEmployeeResponse>>
    {
        private readonly IEmployeeRepository _repo;
        public DeleteEmployeeHandler(IEmployeeRepository repo) { _repo = repo; }
 
        public async Task<BaseResponse<DeleteEmployeeResponse>> Handle(DeleteEmployeeRequest request, CancellationToken ct)
        {
            if (request?.RequestParam == null) throw new BadRequestException(Messaging.InvalidRequest);
 
            var existing = await _repo.GetEmployeeAsync(new GetAllEmployeesRequest
            {
                RequestParam = new GetAllEmployeesDto { EmployeeId = request.RequestParam.EmployeeId }
            }) ?? throw new NotFoundException(string.Format(Messaging.NotFound, nameof(Employee)));
 
            await _repo.DeleteItemAsync(existing.Id);
 
            return new BaseResponse<DeleteEmployeeResponse>
            {
                Data = new DeleteEmployeeResponse { EmployeeId = existing.Id },
                StatusCode = StatusCodes.Status200OK,
                Message = string.Format(Messaging.Delete, nameof(Employee)),
                Success = true
            };
        }
    }
}
 