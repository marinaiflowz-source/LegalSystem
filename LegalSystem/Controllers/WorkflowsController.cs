using LegalSystem.Attributes;
using LegalSystem.DataAccess;
using LegalSystem.DTOs;
using LegalSystem.Enums;
using LegalSystem.Helpers;
using LegalSystem.Models;
using LegalSystem.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LegalSystem.Controllers
{
    [Route("workflows")]
    [ApiController]
    public class WorkflowsController : ControllerBase
    {
        private readonly UnitOfWork unitOfWork;
        private readonly HttpContextProvider httpContext;

        public WorkflowsController(UnitOfWork unitOfWork, HttpContextProvider httpContext)
        {
            this.unitOfWork = unitOfWork;
            this.httpContext = httpContext;
        }

        [HttpGet]
        [RequiredPermission("workflows.get")]
        [ProducesResponseType(typeof(Response<List<WorkflowQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = new Response<List<WorkflowQuery>>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var data = await unitOfWork.RefCaseTypeRepository
                .GetAllQuerable()
                .Include(e => e.WorkflowSteps).ThenInclude(s => s.CaseStatus)
                .Select(e => new WorkflowQuery
                {
                    CaseTypeId = e.Id,
                    CaseTypeName = e.NameEN,

                    Steps = e.WorkflowSteps
                        .OrderBy(s => s.Order)
                        .Select(s => new WorkflowStepQuery
                        {
                            StatusId = s.CaseStatusId,
                            StatusName = s.CaseStatus != null
                                ? s.CaseStatus.NameEN
                                : string.Empty,
                            Order = s.Order
                        })
                        .ToList()
                })
                .ToListAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        [HttpGet("{caseTypeId}")]
        [RequiredPermission("workflows.get.one")]
        [ProducesResponseType(typeof(Response<WorkflowQuery>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByCaseTypeIdAsync(int caseTypeId)
        {
            var result = new Response<WorkflowQuery>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var data = await unitOfWork.RefCaseTypeRepository
                .GetAllQuerable()
                .Where(e => e.Id == caseTypeId)
                .Include(e => e.WorkflowSteps).ThenInclude(s => s.CaseStatus)
                .Select(e => new WorkflowQuery
                {
                    CaseTypeId = e.Id,
                    CaseTypeName = e.NameEN,

                    Steps = e.WorkflowSteps
                        .OrderBy(s => s.Order)
                        .Select(s => new WorkflowStepQuery
                        {
                            StatusId = s.CaseStatusId,
                            StatusName = s.CaseStatus != null
                                ? s.CaseStatus.NameEN
                                : string.Empty,
                            Order = s.Order
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        [HttpPost("create")]
        [RequiredPermission("workflows.create")]
        [ProducesResponseType(typeof(Response<long?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAsync([FromBody] WorkflowCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            // Validate steps
            if (!model.Steps.Any())
            {
                result.Status = (int)ResponseEnum.BadRequest;
                result.Title = "Workflow steps are required";

                return StatusCode(result.Status, result);
            }

            // Validate duplicate statuses
            if (model.Steps.GroupBy(x => x.StatusId).Any(g => g.Count() > 1))
            {
                result.Status = (int)ResponseEnum.BadRequest;
                result.Title = "Duplicate statuses are not allowed";

                return StatusCode(result.Status, result);
            }

            // Validate order sequence
            var orders = model.Steps
                .Select(x => x.Order)
                .OrderBy(x => x)
                .ToList();

            var expectedOrders = Enumerable.Range(1, model.Steps.Count).ToList();

            if (!orders.SequenceEqual(expectedOrders))
            {
                result.Status = (int)ResponseEnum.BadRequest;
                result.Title = "Order must start from 1 and be sequential without gaps";

                return StatusCode(result.Status, result);
            }

            // Remove old workflow if exists
            var existingWorkflow = await unitOfWork.WorkflowRepository
                .GetAllQuerable()
                .Where(x => x.CaseTypeId == model.CaseTypeId)
                .ToListAsync();

            if (existingWorkflow.Any())
            {
                var anyActiveCase = await unitOfWork.CaseRepository.GetAllQuerable().Where(e => e.TypeId == model.CaseTypeId && !e.IsCompleted).AnyAsync();
                if (anyActiveCase)
                {
                    result.Status = (int)ResponseEnum.BadRequest;
                    result.Title = "Cannot update workflow while there are active cases for this case type";

                    return StatusCode(result.Status, result);
                }

                unitOfWork.WorkflowRepository.DeleteRange(existingWorkflow);
            }

            // Create workflow
            var createdOn = DateTime.Now;
            var entities = model.Steps
                .Select(x => new TblWorkFlow
                {
                    CaseTypeId = model.CaseTypeId,
                    CaseStatusId = x.StatusId,
                    Order = x.Order,

                    CreatedOn = createdOn,
                    CreatedById = currentUser.UserId,
                    CreatedByName = currentUser.UserName,
                })
                .ToList();

            await unitOfWork.WorkflowRepository.AddRangeAsync(entities);

            await unitOfWork.CompleteAsync();

            result.Data = model.CaseTypeId;

            return StatusCode(result.Status, result);
        }
    }
}
