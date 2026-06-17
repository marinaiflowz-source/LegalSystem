using LegalSystem.Attributes;
using LegalSystem.DataAccess;
using LegalSystem.DTOs;
using LegalSystem.Enums;
using LegalSystem.Helpers;
using LegalSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LegalSystem.Controllers
{
    [Route("case-teams")]
    [ApiController]
    public class CaseTeamController : ControllerBase
    {
        private readonly UnitOfWork unitOfWork;
        private readonly HttpContextProvider httpContext;

        public CaseTeamController(UnitOfWork unitOfWork, HttpContextProvider httpContext)
        {
            this.unitOfWork = unitOfWork;
            this.httpContext = httpContext;
        }

        // ========================= GET ALL =========================
        [HttpGet("{caseId}")]
        [RequiredPermission("caseTeams.get")]
        [ProducesResponseType(typeof(Response<IEnumerable<CaseTeamQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync(long caseId)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<IEnumerable<CaseTeamQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.CaseTeamRepository.GetAllQuerable()
                .Include(e => e.User).ThenInclude(e => e!.Type)
                .Include(e => e.Case).ThenInclude(e => e!.Team)
                .Where(e => e.CaseId == caseId)
                .Where(e => currentUser.IsSuperAdmin || e.Case!.Team.Select(t => t.UserId).Contains(currentUser.UserId))
                .AsNoTracking();



            // Projection
            var data = await rows.Select(x => new CaseTeamQuery
            {
                Id = x.Id,
                CaseId = x.CaseId,
                User = x.User == null ? null : new UserSummaryView
                {
                    Id = x.UserId,
                    NameEn = x.User!.NameEn,
                    NameAr = x.User.NameAr,
                    Email = x.User.Email,
                    Type = x.User.Type!.NameEN
                },
                CreatedOn = x.CreatedOn,
                CreatedById = x.CreatedById,
                CreatedByName = x.CreatedByName,

                UpdatedOn = x.UpdatedOn,
                UpdatedByName = x.UpdatedByName,
                UpdatedById = x.UpdatedById,

            }).ToListAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            //var obj = new
            //{
            //    Id = caseId
            //};

            //string json = JsonSerializer.Serialize(obj);
            //var entityDocument = new TblAuditLog
            //{
            //    UserId = currentUser.Email,
            //    Type = "Get",
            //    TableName = "caseTeams",
            //    ActionType = "Get",
            //    DateTime = DateTime.Now,
            //    OldValues = null,
            //    NewValues = null,
            //    AffectedColumns = null,
            //    PrimaryKey = json,
            //    IsArchived = false,

            //};

            //await unitOfWork.AuditLogRepository.AddAsync(entityDocument);

            //await unitOfWork.CompleteAsync();


            return StatusCode(result.Status, result);
        }

        // ========================= CREATE =========================
        [HttpPost("create")]
        [RequiredPermission("caseTeams.create")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAsync([FromBody] CaseTeamCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var entity = new TblCaseTeam
            {
                CaseId = model.CaseId,
                UserId = model.UserId,
                CreatedOn = DateTime.Now,
                CreatedById = currentUser.UserId,
                CreatedByName = currentUser.UserName,
            };

            await unitOfWork.CaseTeamRepository.AddAsync(entity);

            var user = await unitOfWork.UserRepository.GetByIdAsync(model.UserId);
            await unitOfWork.CaseAuditLogRepository.AddAsync(new TblCaseAuditLog
            {
                CaseId = model.CaseId,
                Comment = $"Case team member added. User: {user?.NameEn}",
                CreatedOn = DateTime.Now,
                CreatedById = currentUser.UserId,
                CreatedByName = currentUser.UserName,
            });

            await unitOfWork.CompleteAsync();

            result.Data = entity.Id;
            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "Create",
                TableName = "Case Teams",
                ActionType = "Create",
                DateTime = DateTime.Now,
                OldValues = null,
                NewValues = null,
                AffectedColumns = null,
                PrimaryKey = json,
                IsArchived = false,

            };

            await unitOfWork.AuditLogRepository.AddAsync(entityDocument);

            await unitOfWork.CompleteAsync();


            return StatusCode(result.Status, result);
        }

        // ========================= DELETE =========================
        [HttpPost("{id}/delete")]
        [RequiredPermission("caseTeams.delete")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.CaseTeamRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            var user = await unitOfWork.UserRepository.GetByIdAsync(entity.UserId);
            await unitOfWork.CaseAuditLogRepository.AddAsync(new TblCaseAuditLog
            {
                CaseId = entity.CaseId,
                Comment = $"Case team member removed. User: {user?.NameEn}",
                CreatedOn = DateTime.Now,
                CreatedById = currentUser.UserId,
                CreatedByName = currentUser.UserName,
            });

            unitOfWork.CaseTeamRepository.Delete(entity);

            await unitOfWork.CompleteAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Deleted";
            result.Data = true;
            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "Delete",
                TableName = "Case Teams",
                ActionType = "Delete",
                DateTime = DateTime.Now,
                OldValues = null,
                NewValues = null,
                AffectedColumns = null,
                PrimaryKey = json,
                IsArchived = false,

            };

            await unitOfWork.AuditLogRepository.AddAsync(entityDocument);

            await unitOfWork.CompleteAsync();



            return StatusCode(result.Status, result);
        }
    }
}
