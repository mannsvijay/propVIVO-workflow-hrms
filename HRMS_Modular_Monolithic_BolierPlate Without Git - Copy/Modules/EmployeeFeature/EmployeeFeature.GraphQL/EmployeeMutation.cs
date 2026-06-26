
// ================================================================
// FILE: Modules/EmployeeFeature/EmployeeFeature.GraphQL/EmployeeMutation.cs
// ================================================================
using EmployeeFeature.Application.DTO;
using HRMS.Shared.Application.DTOs;
using HRMS.Shared.Application.GraphQL;
using HotChocolate;
using HotChocolate.Types;
using MediatR;
 
namespace EmployeeFeature.GraphQL
{
    [ExtendObjectType(typeof(Mutation))]
    public class EmployeeMutation
    {
        [GraphQLName("createEmployee")]
        public async Task<BaseResponse<CreateEmployeeResponse>> CreateEmployee(CreateEmployeeRequest request, [Service] IMediator mediator)
            => await mediator.Send(request);
 
        [GraphQLName("updateEmployee")]
        public async Task<BaseResponse<UpdateEmployeeResponse>> UpdateEmployee(UpdateEmployeeRequest request, [Service] IMediator mediator)
            => await mediator.Send(request);
 
        [GraphQLName("deleteEmployee")]
        public async Task<BaseResponse<DeleteEmployeeResponse>> DeleteEmployee(DeleteEmployeeRequest request, [Service] IMediator mediator)
            => await mediator.Send(request);
    }
 
    [ExtendObjectType(typeof(Query))]
    public class EmployeeQuery
    {
        [GraphQLName("getAllEmployees")]
        public async Task<BaseResponsePagination<GetAllEmployeesResponse>> GetAllEmployees(GetAllEmployeesRequest request, [Service] IMediator mediator)
            => await mediator.Send(request);
    }
}