using LegalSystem.Attributes;
using LegalSystem.DataAccess;
using LegalSystem.DTOs;
using LegalSystem.Helpers;
using LegalSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LegalSystem.Controllers
{
    [Route("Inventory")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ReferancesService referancesService;
        private readonly HttpContextProvider httpContext;
        private readonly CrmService crmService;

        public InventoryController(UnitOfWork unitOfWork, HttpContextProvider httpContext, ReferancesService referancesService, CrmService crmService)
        {
            this.unitOfWork = unitOfWork;
            this.httpContext = httpContext;
            this.referancesService = referancesService;
            this.crmService = crmService;
        }

        [HttpGet]
        [RequiredPermission("cases.get")]
        [ProducesResponseType(typeof(QueryResult<LegalInventoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync([FromQuery] InventoryFilter filter)
        {
            var result = new QueryResult<LegalInventoryDto>
            {
                Status = StatusCodes.Status200OK,
                Title = "Data Retrieved"
            };

            var query = crmService.GetInventoryQueryable();

            // =====================
            // Filtering
            // =====================
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                query = query.Where(x =>
                    (x.LeadID + x.ProjectCode + x.ProjectName + x.UnitNumber)
                    .Contains(filter.SearchText));
            }

            if (!string.IsNullOrWhiteSpace(filter.ProjectCode))
                query = query.Where(x => x.ProjectCode.Trim().Contains(filter.ProjectCode.Trim()));

            if (!string.IsNullOrWhiteSpace(filter.UnitNumber))
                query = query.Where(x => x.UnitNumber.Trim().Contains(filter.UnitNumber.Trim()));

            if (!string.IsNullOrWhiteSpace(filter.UnitType))
                query = query.Where(x => x.UnitType == filter.UnitType);

            if (!string.IsNullOrWhiteSpace(filter.LeadStatus))
                query = query.Where(x => x.LeadStatus == filter.LeadStatus);


            var listBeforePaging = query.ToList();
            var totalItems = listBeforePaging.Count;

            if (totalItems == 0)
            {
                result.Title = "No Data Found";
                result.Data.Count = 0;
                result.Data.Rows = new List<LegalInventoryDto>();
                return Ok(result);
            }


            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                filter.Sort = "LeadID";
                filter.IsAscending = false;
            }

            listBeforePaging = filter.Sort switch
            {
                "ProjectName" => filter.IsAscending
                    ? listBeforePaging.OrderBy(x => x.ProjectName).ToList()
                    : listBeforePaging.OrderByDescending(x => x.ProjectName).ToList(),

                "ProjectCode" => filter.IsAscending
                    ? listBeforePaging.OrderBy(x => x.ProjectCode).ToList()
                    : listBeforePaging.OrderByDescending(x => x.ProjectCode).ToList(),

                "LeadStatus" => filter.IsAscending
                    ? listBeforePaging.OrderBy(x => x.LeadStatus).ToList()
                    : listBeforePaging.OrderByDescending(x => x.LeadStatus).ToList(),

                _ => filter.IsAscending
                    ? listBeforePaging.OrderBy(x => x.LeadID).ToList()
                    : listBeforePaging.OrderByDescending(x => x.LeadID).ToList(),
            };

            // =====================
            // Paging
            // =====================
            var pagedData = listBeforePaging;

            if (filter.Size > 0)
            {
                pagedData = listBeforePaging
                    .Skip(filter.Index * filter.Size)
                    .Take(filter.Size)
                    .ToList();
            }


            result.Data.Count = totalItems;
            result.Data.Rows = pagedData;

            return Ok(result);
        }


        [HttpGet("projects")]
        public IActionResult GetProjectsAsync()
        {
            try
            {
                var data = crmService.GetInventoryQueryable()
                    .Where(x => !string.IsNullOrWhiteSpace(x.ProjectCode))
                    .Select(x => new
                    {
                        ProjectCode = x.ProjectCode.Trim(),
                        ProjectName = x.ProjectName
                    })
                    .ToList()
                    .GroupBy(x => x.ProjectCode)
                    .Select(g => new ProjectLookupDto
                    {
                        ProjectCode = g.Key,
                        ProjectName = g.First().ProjectName?.Trim() ?? string.Empty
                    })
                    .OrderBy(x => x.ProjectName)
                    .ToList();

                return Ok(new Response<List<ProjectLookupDto>>
                {
                    Status = StatusCodes.Status200OK,
                    Title = "Data Retrieved",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }


    }
}
