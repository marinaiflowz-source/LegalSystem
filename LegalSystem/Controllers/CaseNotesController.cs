using LegalSystem.Attributes;
using LegalSystem.DataAccess;
using LegalSystem.DTOs;
using LegalSystem.Enums;
using LegalSystem.Helpers;
using LegalSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LegalSystem.Controllers
{
    [Route("case-notes")]
    [ApiController]
    public class CaseNotesController : ControllerBase
    {
        private readonly UnitOfWork unitOfWork;
        private readonly HttpContextProvider httpContext;

        public CaseNotesController(UnitOfWork unitOfWork, HttpContextProvider httpContext)
        {
            this.unitOfWork = unitOfWork;
            this.httpContext = httpContext;
        }

        // ========================= GET ALL =========================
        [HttpGet("{caseId}")]
        [RequiredPermission("caseNotes.get")]
        [ProducesResponseType(typeof(Response<IEnumerable<CaseNoteQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync(long caseId)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<IEnumerable<CaseNoteQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.CaseNoteRepository.GetAllQuerable()
                .Include(e => e.Case).ThenInclude(c => c!.Team)
                .Where(x => x.CaseId == caseId)
                //.Where(e => currentUser.IsSuperAdmin || e.Case!.Team.Select(t => t.UserId).Contains(currentUser.UserId))
                .AsNoTracking();

            var data = await rows.Select(x => new CaseNoteQuery
            {
                Id = x.Id,
                CaseId = x.CaseId,
                Note = x.Note
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
            //    TableName = "caseNotes",
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
        [RequiredPermission("caseNotes.create")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAsync([FromBody] CaseNoteCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var entity = new TblCaseNote
            {
                CaseId = model.CaseId,
                Note = model.Note,

                CreatedOn = DateTime.Now,
                CreatedById = currentUser.UserId,
                CreatedByName = currentUser.UserName,
            };

            await unitOfWork.CaseNoteRepository.AddAsync(entity);

            await unitOfWork.CaseAuditLogRepository.AddAsync(new TblCaseAuditLog
            {
                CaseId = model.CaseId,
                Comment = $"Case note created. Note: {model.Note}",
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
                TableName = "case Notes",
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

        // ========================= UPDATE =========================
        [HttpPost("{id}/update")]
        [RequiredPermission("caseNotes.update")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] CaseNoteUpdate model)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.CaseNoteRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);
            var obj = new
            {
                Id = id
            };

            string json = JsonSerializer.Serialize(obj);
            var updatedOn = DateTime.Now;
            var auditLogEntities = new List<TblCaseAuditLog>();
            var auditLogs = new List<TblAuditLog>();
            //var entityDocument = new TblAuditLog();
            void AddAuditLog(string fieldName, object? oldValue, object? newValue)
            {
                auditLogEntities.Add(new TblCaseAuditLog
                {
                    CaseId = entity.CaseId,
                    Comment = $"Note {fieldName} updated from {oldValue ?? "NULL"} to {newValue ?? "NULL"}",
                    CreatedOn = updatedOn,
                    CreatedById = currentUser.UserId,
                    CreatedByName = currentUser.UserName,
                });

               
                //auditLogs.AddRange( new TblAuditLog
                //{
                //    UserId = currentUser.Email,
                //    Type = "update",
                //    TableName = "case Notes",
                //    ActionType = "update",
                //    DateTime = DateTime.Now,
                //    OldValues = oldValue.ToString(),
                //    NewValues = newValue.ToString(),
                //    AffectedColumns = null,
                //    PrimaryKey = json,
                //    IsArchived = false,

                //});


            }

            if (!string.IsNullOrWhiteSpace(model.Note) &&
                entity.Note != model.Note)
            {
                AddAuditLog("Note", entity.Note, model.Note);
                entity.Note = model.Note;
                auditLogs.AddRange(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "case Notes",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.Note.ToString(),
                    NewValues = model.Note.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }

            entity.UpdatedOn = updatedOn;
            entity.UpdatedById = currentUser.UserId;
            entity.UpdatedByName = currentUser.UserName;

            unitOfWork.CaseNoteRepository.Update(entity);

            if (auditLogEntities.Any())
                await unitOfWork.CaseAuditLogRepository.AddRangeAsync(auditLogEntities);

            await unitOfWork.AuditLogRepository.AddRangeAsync(auditLogs);
            await unitOfWork.CompleteAsync();
           ;

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Updated";
            result.Data = true;



            return StatusCode(result.Status, result);
        }

        // ========================= DELETE =========================
        [HttpPost("{id}/delete")]
        [RequiredPermission("caseNotes.delete")]
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

            var entity = await unitOfWork.CaseNoteRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            await unitOfWork.CaseAuditLogRepository.AddAsync(new TblCaseAuditLog
            {
                CaseId = entity.CaseId,
                Comment = $"Case note deleted. Note: {entity.Note}",
                CreatedOn = DateTime.Now,
                CreatedById = currentUser.UserId,
                CreatedByName = currentUser.UserName,
            });

            unitOfWork.CaseNoteRepository.Delete(entity);

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
                TableName = "case Notes",
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