using LegalSystem.Attributes;
using LegalSystem.DTOs;
using LegalSystem.Enums;
using LegalSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace LegalSystem.Controllers
{
    [Route("orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly CrmServiceOld crmService;

        public OrdersController(CrmServiceOld crmService)
        {
            this.crmService = crmService;
        }
        // ========================= GET ALL =========================
        [HttpGet]
        [RequiredPermission("orders.get")]
        [ProducesResponseType(typeof(QueryResult<CrmOrderQuery>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync([FromQuery] OrderFilter model)
        {
            var result = new QueryResult<CrmOrderQuery>()
            {
                Status = (int)ResponseEnum.Succeeded,
                Title = "Data Retrieved"
            };

            var filter = new CrmOrderFilter
            {
                SearchText = model.SearchText,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo,
                PageNumber = model.Index,
                PageSize = model.Size,
            };
            result.Data.Count = await crmService.GetCrmOrderCountAsync(filter);
            result.Data.Rows = await crmService.GetCrmOrdersAsync(filter);
            return StatusCode(result.Status, result);
        }

        [HttpGet("{refId}/buyers")]
        [RequiredPermission("orders.buyers")]
        [ProducesResponseType(typeof(Response<IEnumerable<CrmOrderBuyerQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBuyersAsync(int refId)
        {
            var result = new Response<IEnumerable<CrmOrderBuyerQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var data = await crmService.GetCrmOrderBuyersAsync(refId);

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }
    }
}
