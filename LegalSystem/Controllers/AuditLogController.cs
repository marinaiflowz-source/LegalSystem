using LegalSystem.Attributes;
using LegalSystem.DataAccess;
using LegalSystem.DataAccess.Migrations;
using LegalSystem.DTOs;
using LegalSystem.Enums;
using LegalSystem.Helpers;
using LegalSystem.Models;
using LegalSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static LegalSystem.DTOs.AuditEntry;

namespace LegalSystem.Controllers
{
    [Route("audit-log")]
    [ApiController]
    public class AuditLogController : ControllerBase
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ReferancesService referancesService;
        private readonly HttpContextProvider httpContext;

        public AuditLogController(UnitOfWork unitOfWork, HttpContextProvider httpContext, ReferancesService referancesService)
        {
            this.unitOfWork = unitOfWork;
            this.httpContext = httpContext;
            this.referancesService = referancesService;
        }

        // ========================= GET ALL =========================
        [HttpGet]
        [RequiredPermission("cases.get")]
        [ProducesResponseType(typeof(QueryResult<CaseQuery>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
       
        public async Task<IActionResult> GetAsync([FromQuery] BaseRefFilter filter)
        {
            var result = new Response<IEnumerable<AuditCommand>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.AuditLogRepository.GetAllQuerable()
                .AsNoTracking();

         
            var data = await rows.Select(e => new AuditCommand
            {
              UserId =e.UserId,
              Type =e.Type,
              TableName =e.TableName,
              DateTime =e.DateTime,
              OldValues =e.OldValues,
              NewValues =e.NewValues,
              AffectedColumns =e.AffectedColumns,
              PrimaryKey =e.PrimaryKey,
              IsArchived =e.IsArchived,
              Comment =e.Comment


            }).ToListAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAsync()
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var entity = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "login",
                TableName = "Users",
                DateTime = DateTime.Now,
                OldValues =null,
                NewValues =null,
                AffectedColumns =null,
                PrimaryKey =null,
                IsArchived = false,
                Comment=null
            };

            await unitOfWork.AuditLogRepository.AddAsync(entity);

            await unitOfWork.CompleteAsync();

            result.Data = entity.Id;

            return StatusCode(result.Status, result);
        }




    }
}
