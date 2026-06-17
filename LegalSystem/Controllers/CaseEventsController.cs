using LegalSystem.Attributes;
using LegalSystem.DataAccess;
using LegalSystem.DTOs;
using LegalSystem.Enums;
using LegalSystem.Helpers;
using LegalSystem.Models;
using LegalSystem.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LegalSystem.Controllers
{
    [Route("case-events")]
    [ApiController]
    public class CaseEventController : ControllerBase
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ReferancesService referancesService;
        private readonly HttpContextProvider httpContext;

        public CaseEventController(UnitOfWork unitOfWork, HttpContextProvider httpContext, ReferancesService referancesService)
        {
            this.unitOfWork = unitOfWork;
            this.httpContext = httpContext;
            this.referancesService = referancesService;
        }

        // ========================= GET ALL =========================
        [HttpGet("calender")]
        [RequiredPermission("caseEvents.calender")]
        [ProducesResponseType(typeof(Response<IEnumerable<CaseEventQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync([FromQuery] CaseEventFilter model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<IEnumerable<CaseEventQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Succeeded,
                Title = "Data Retrieved"
            };

            var rows = unitOfWork.CaseEventRepository.GetAllQuerable()
                .Include(e => e.Court)
                .Include(e => e.Type)
                .Include(e => e.Case).ThenInclude(c => c!.Team)
                .Where(e => e.Date >= model.DueDateFrom && e.Date <= model.DueDateTo)
                .Where(e => currentUser.IsSuperAdmin || e.Case!.Team.Select(t => t.UserId).Contains(currentUser.UserId))
                .AsNoTracking();

            var data = await rows.Select(x => new CaseEventQuery
            {
                Id = x.Id,
                CaseId = x.CaseId,

                Title = x.Title,
                Date = x.Date,

                Court = x.Court == null ? null : new SummaryView
                {
                    Id = x.Court.Id,
                    Name = x.Court.NameEN,
                },

                Type = x.Type == null ? null : new SummaryView
                {
                    Id = x.Type.Id,
                    Name = x.Type.NameEN,
                },

                Address = x.Address,
                Latitude = x.Latitude,
                Longitude = x.Longitude,

                CreatedOn = x.CreatedOn,
                CreatedById = x.CreatedById,
                CreatedByName = x.CreatedByName,

                UpdatedOn = x.UpdatedOn,
                UpdatedById = x.UpdatedById,
                UpdatedByName = x.UpdatedByName,

            }).ToListAsync();

            result.Data = data;
         
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "calender",
                TableName = "case-events",
                ActionType = "calender",
                DateTime = DateTime.Now,
                OldValues = null,
                NewValues = null,
                AffectedColumns = null,
                PrimaryKey = null,
                IsArchived = false,

            };

            await unitOfWork.AuditLogRepository.AddAsync(entityDocument);
            await unitOfWork.CompleteAsync();
            return StatusCode(result.Status, result);
        }

        // ========================= GET BY CASE =========================
        [HttpGet("{caseId}")]
        [RequiredPermission("caseEvents.get")]
        [ProducesResponseType(typeof(Response<IEnumerable<CaseEventQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync(long caseId)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<IEnumerable<CaseEventQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.CaseEventRepository.GetAllQuerable()
                .Include(e => e.Court)
                .Include(e => e.Type)
                .Include(e => e.Case).ThenInclude(c => c!.Team)
                .Where(e => e.CaseId == caseId)
                .Where(e => currentUser.IsSuperAdmin || e.Case!.Team.Select(t => t.UserId).Contains(currentUser.UserId))
                .AsNoTracking();

            var data = await rows.Select(x => new CaseEventQuery
            {
                Id = x.Id,
                CaseId = x.CaseId,

                Title = x.Title,
                Date = x.Date,

                Court = x.Court == null ? null : new SummaryView
                {
                    Id = x.Court.Id,
                    Name = x.Court.NameEN,
                },

                Type = x.Type == null ? null : new SummaryView
                {
                    Id = x.Type.Id,
                    Name = x.Type.NameEN,
                },

                Address = x.Address,
                Latitude = x.Latitude,
                Longitude = x.Longitude,

                CreatedOn = x.CreatedOn,
                CreatedById = x.CreatedById,
                CreatedByName = x.CreatedByName,

                UpdatedOn = x.UpdatedOn,
                UpdatedById = x.UpdatedById,
                UpdatedByName = x.UpdatedByName,

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
            //    Type = "calender",
            //    TableName = "GetCase",
            //    ActionType = "calender",
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
        [RequiredPermission("caseEvents.create")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAsync([FromBody] CaseEventCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var entity = new TblCaseEvent
            {
                CaseId = model.CaseId,
                Title = model.Title,
                Date = model.Date,
                CourtId = model.CourtId,
                TypeId = model.TypeId,

                Address = model.Address,
                Latitude = model.Latitude,
                Longitude = model.Longitude,

                CreatedOn = DateTime.Now,
                CreatedById = currentUser.UserId,
                CreatedByName = currentUser.UserName,
            };

            await unitOfWork.CaseEventRepository.AddAsync(entity);

            await unitOfWork.CaseAuditLogRepository.AddAsync(new TblCaseAuditLog
            {
                CaseId = model.CaseId,
                Comment = $"Case event created. Title: {model.Title}, Date: {model.Date}, CourtId: {model.CourtId}, TypeId: {model.TypeId}",
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
                Type = "create",
                TableName = "caseEvents",
                ActionType = "create",
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
        [RequiredPermission("caseEvents.update")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] CaseEventUpdate model)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.CaseEventRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            var refs = await referancesService.GetAllReferancesAsync();
            var updatedOn = DateTime.Now;
            var auditLogEntities = new List<TblCaseAuditLog>();
            //var entityDocument = new TblAuditLog();
            var auditLogs = new List<TblAuditLog>();
            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            void AddAuditLog(string fieldName, object? oldValue, object? newValue)
            {
                auditLogEntities.Add(new TblCaseAuditLog
                {
                    CaseId = entity.CaseId,
                    Comment = $"Event {fieldName} updated from {oldValue ?? "NULL"} to {newValue ?? "NULL"}",
                    CreatedOn = updatedOn,
                    CreatedById = currentUser.UserId,
                    CreatedByName = currentUser.UserName,
                });

                
                auditLogs.Add( new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "Update",
                    TableName = "caseEvents",
                    ActionType = "Update",
                    DateTime = DateTime.Now,
                    OldValues = oldValue.ToString(),
                    NewValues = newValue.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false
                });

              

            }

            if (!string.IsNullOrWhiteSpace(model.Title) &&
                entity.Title != model.Title)
            {
                AddAuditLog("Title", entity.Title, model.Title);
                entity.Title = model.Title;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "Update",
                    TableName = "caseEvents",
                    ActionType = "Update",
                    DateTime = DateTime.Now,
                    OldValues = entity.Title.ToString(),
                    NewValues = model.Title.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false
                });
            }

            if (model.Date.HasValue &&
                entity.Date.Date != model.Date.Value.Date)
            {
                AddAuditLog("Date", entity.Date, model.Date.Value);
                entity.Date = model.Date.Value;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "Update",
                    TableName = "caseEvents",
                    ActionType = "Update",
                    DateTime = DateTime.Now,
                    OldValues = entity.Date.Date.ToString(),
                    NewValues = model.Date.Value.Date.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false
                });
            }

            if (model.CourtId.HasValue &&
                entity.CourtId != model.CourtId.Value)
            {
                AddAuditLog("Court", refs.Courts.FirstOrDefault(r => r.Id == entity.CourtId)?.NameEN, refs.Courts.FirstOrDefault(r => r.Id == model.CourtId.Value)?.NameEN);
                entity.CourtId = model.CourtId.Value;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "Update",
                    TableName = "caseEvents",
                    ActionType = "Update",
                    DateTime = DateTime.Now,
                    OldValues = entity.CourtId.ToString(),
                    NewValues = model.CourtId.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false
                });
            }

            if (model.TypeId.HasValue &&
                entity.TypeId != model.TypeId.Value)
            {
                AddAuditLog("Type", refs.EventTypes.FirstOrDefault(r => r.Id == entity.TypeId)?.NameEN, refs.EventTypes.FirstOrDefault(r => r.Id == model.TypeId.Value)?.NameEN);
                entity.TypeId = model.TypeId.Value;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "Update",
                    TableName = "caseEvents",
                    ActionType = "Update",
                    DateTime = DateTime.Now,
                    OldValues = entity.TypeId.ToString(),
                    NewValues = model.TypeId.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false
                });
            }

            if (model.Address != null &&
                entity.Address != model.Address)
            {
                AddAuditLog("Address", entity.Address, model.Address);
                entity.Address = model.Address;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "Update",
                    TableName = "caseEvents",
                    ActionType = "Update",
                    DateTime = DateTime.Now,
                    OldValues = entity.Address.ToString(),
                    NewValues = model.Address.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false
                });
            }

            if (model.Latitude.HasValue &&
                entity.Latitude != model.Latitude.Value)
            {
                AddAuditLog("Latitude", entity.Latitude, model.Latitude.Value);
                entity.Latitude = model.Latitude.Value;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "Update",
                    TableName = "caseEvents",
                    ActionType = "Update",
                    DateTime = DateTime.Now,
                    OldValues = entity.Latitude.ToString(),
                    NewValues = model.Latitude.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false
                });
            }

            if (model.Longitude.HasValue &&
                entity.Longitude != model.Longitude.Value)
            {
                AddAuditLog("Longitude", entity.Longitude, model.Longitude.Value);
                entity.Longitude = model.Longitude.Value;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "Update",
                    TableName = "caseEvents",
                    ActionType = "Update",
                    DateTime = DateTime.Now,
                    OldValues = entity.Longitude.ToString(),
                    NewValues = model.Longitude.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false
                });
            }

            entity.UpdatedOn = updatedOn;
            entity.UpdatedById = currentUser.UserId;
            entity.UpdatedByName = currentUser.UserName;

            unitOfWork.CaseEventRepository.Update(entity);

            if (auditLogEntities.Any())
                await unitOfWork.CaseAuditLogRepository.AddRangeAsync(auditLogEntities);
            await unitOfWork.AuditLogRepository.AddRangeAsync(auditLogs);
            await unitOfWork.CompleteAsync();
            
            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Updated";
            result.Data = true;

            return StatusCode(result.Status, result);
        }

        // ========================= DELETE =========================
        [HttpPost("{id}/delete")]
        [RequiredPermission("caseEvents.delete")]
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

            var entity = await unitOfWork.CaseEventRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            await unitOfWork.CaseAuditLogRepository.AddAsync(new TblCaseAuditLog
            {
                CaseId = entity.CaseId,
                Comment = $"Case event deleted. Title: {entity.Title}, Date: {entity.Date}, CourtId: {entity.CourtId}, TypeId: {entity.TypeId}",
                CreatedOn = DateTime.Now,
                CreatedById = currentUser.UserId,
                CreatedByName = currentUser.UserName,
            });

            unitOfWork.CaseEventRepository.Delete(entity);

            await unitOfWork.CompleteAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Deleted";
            result.Data = true;



            var obj = new
            {
                Id = id
            };

            string json = JsonSerializer.Serialize(obj);
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "delete",
                TableName = "caseEvents",
                ActionType = "delete",
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
