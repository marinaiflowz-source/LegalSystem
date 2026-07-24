using ClosedXML.Excel;
using LegalSystem.Attributes;
using LegalSystem.DataAccess;
using LegalSystem.DTOs;
using LegalSystem.Enums;
using LegalSystem.Helpers;
using LegalSystem.Models;
using LegalSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.AccessControl;
using System.Text.Json;

namespace LegalSystem.Controllers
{
    [Route("case-tasks")]
    [ApiController]
    public class CaseTaskController : ControllerBase
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ReferancesService referancesService;
        private readonly HttpContextProvider httpContext;

        public CaseTaskController(UnitOfWork unitOfWork, HttpContextProvider httpContext, ReferancesService referancesService)
        {
            this.unitOfWork = unitOfWork;
            this.httpContext = httpContext;
            this.referancesService = referancesService;
        }

        // ========================= GET ALL =========================
        [HttpGet]
        [RequiredPermission("caseTasks.get")]
        [ProducesResponseType(typeof(QueryResult<CaseTaskQuery>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync([FromQuery] CaseTaskFilter filter)
        {
            var currentUser =await httpContext.GetCurrentUser();

            var result = new QueryResult<CaseTaskQuery>()
            {
                Status = (int)ResponseEnum.Succeeded,
                Title = "Data Retrieved"
            };

            var rows = unitOfWork.CaseTaskRepository.GetAllQuerable()
                .Include(e => e.Case).ThenInclude(c => c!.Court)
                .Include(e => e.Case).ThenInclude(c => c!.Team)
                .Include(e => e.AssignedUser).ThenInclude(e => e!.Type)
                .Include(e => e.Priority)
                .Include(e => e.Status)
                .Include(e => e.RefTaskType)
                //.Where(e => currentUser.IsSuperAdmin)
                .AsNoTracking();

            // Filtering
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                rows = rows.Where(x =>
                    (x.Title).Contains(filter.SearchText));
            }

            if (filter.StatusId.HasValue)
                rows = rows.Where(x => x.StatusId == filter.StatusId.Value);

            if (filter.DueDateFrom.HasValue)
                rows = rows.Where(x => x.DueDate >= filter.DueDateFrom.Value);

            if (filter.DueDateTo.HasValue)
                rows = rows.Where(x => x.DueDate <= filter.DueDateTo.Value);

            var totalItems = await rows.CountAsync();

            if (totalItems <= 0)
            {
                result.Title = "No Data Found";
                return StatusCode(result.Status, result);
            }

            // Ordering
            if (string.IsNullOrWhiteSpace(filter.SortBy))
            {
                filter.SortBy = "Id";
                filter.IsAscending = false;
            }

            rows = filter.SortBy switch
            {
                "DueDate" => filter.IsAscending ? rows.OrderBy(x => x.DueDate) : rows.OrderByDescending(x => x.DueDate),
                "PriorityId" => filter.IsAscending ? rows.OrderBy(x => x.PriorityId) : rows.OrderByDescending(x => x.PriorityId),
                "CreatedOn" => filter.IsAscending ? rows.OrderBy(x => x.CreatedOn) : rows.OrderByDescending(x => x.CreatedOn),
                _ => filter.IsAscending ? rows.OrderBy(x => x.Id) : rows.OrderByDescending(x => x.Id),
            };

            // Paging
            result.Data.Count = totalItems;

            if (filter.Size > 0)
            {
                rows = rows.Skip(filter.Index * filter.Size).Take(filter.Size);
            }

            // Projection
            var data = rows.Select(x =>
             new CaseTaskQuery
             {
                 Id = x.Id,
                 CaseId = x.CaseId,
                 Title = x.Title,
                 DueDate = x.DueDate,

                 Priority = x.Priority == null ? null : new SummaryView
                 {
                     Id = x.Priority.Id,
                     Name = x.Priority.NameEN,
                 },

                 Status = x.Status == null ? null : new SummaryView
                 {
                     Id = x.Status.Id,
                     Name = x.Status.NameEN,
                 },
                 Court = x.Case!.Court == null ?null : new SummaryView
                 {
                     Id = x.Case.Court.Id,
                     Name = x.Case.Court.NameEN,
                 },

                 User = x.AssignedUser == null ? null : new UserSummaryView
                 {
                     Id = x.AssignedUser.Id,
                     NameEn = x.AssignedUser.NameEn,
                     NameAr = x.AssignedUser.NameAr,
                     Email = x.AssignedUser.Email,
                     Type = x.AssignedUser.Type!.NameEN
                 },
                 
                 CreatedOn = x.CreatedOn,
                 CreatedById = x.CreatedById,
                 CreatedByName = x.CreatedByName,

                 UpdatedOn = x.UpdatedOn,
                 UpdatedById = x.UpdatedById,
                 UpdatedByName = x.UpdatedByName,
                 TaskType = x.RefTaskType == null ? null : new RefTaskTypeView
                 {
                     Id = x.RefTaskType.Id,
                     Name = x.RefTaskType.NameEN,
                 },
                 Note = x.Note
             }
            ).ToList();

            result.Data.Rows = data;
           
            //var entityDocument = new TblAuditLog
            //{
            //    UserId = currentUser.Email,
            //    Type = "GetAll",
            //    TableName = "casetasks",
            //    ActionType = "GetAll",
            //    DateTime = DateTime.Now,
            //    OldValues = null,
            //    NewValues = null,
            //    AffectedColumns = null,
            //    PrimaryKey = null,
            //    IsArchived = false,

            //};

            //await unitOfWork.AuditLogRepository.AddAsync(entityDocument);

            //await unitOfWork.CompleteAsync();


            return StatusCode(result.Status, result);
        }

        // ========================= GET BY CASE =========================
        [HttpGet("{caseId}")]
        [RequiredPermission("caseTasks.getByCase")]
        [ProducesResponseType(typeof(Response<IEnumerable<CaseTaskQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByCaseIdAsync(long caseId)
        {
            var currentUser =await httpContext.GetCurrentUser();
            var result = new Response<IEnumerable<CaseTaskQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.CaseTaskRepository.GetAllQuerable()
                .Include(e => e.AssignedUser).ThenInclude(e => e!.Type)
                .Include(e => e.Case).ThenInclude(e => e!.Court)
                .Include(e => e.Case).ThenInclude(e => e!.Team)
                .Include(e => e.Priority)
                .Include(e => e.Status)
                .Include(e=>e.Reason)
                .Include(e => e.RefTaskType)
                .Where(e => e.CaseId == caseId)
                //.Where(e => currentUser.IsSuperAdmin || e.Case!.Team.Select(t => t.UserId).Contains(currentUser.UserId))
                .AsNoTracking();



            // Projection
            var data = await rows.Select(x => new CaseTaskQuery
            {
                Id = x.Id,
                CaseId = x.CaseId,
                Title = x.Title,
                DueDate = x.DueDate,

                Priority = x.Priority == null ? null : new SummaryView
                {
                    Id = x.Priority.Id,
                    Name = x.Priority.NameEN,
                },

                Status = x.Status == null ? null : new SummaryView
                {
                    Id = x.Status.Id,
                    Name = x.Status.NameEN,
                },
                Court = x.Case!.Court == null ? null : new SummaryView
                {
                    Id = x.Case.Court.Id,
                    Name = x.Case.Court.NameEN,
                },

                User = x.AssignedUser == null ? null : new UserSummaryView
                {
                    Id = x.AssignedUser.Id,
                    NameEn = x.AssignedUser.NameEn,
                    NameAr = x.AssignedUser.NameAr,
                    Email = x.AssignedUser.Email,
                    Type = x.AssignedUser.Type!.NameEN
                },
                IsClosed=x.IsClosed,
                CreatedOn = x.CreatedOn,
                CreatedById = x.CreatedById,
                CreatedByName = x.CreatedByName,

                UpdatedOn = x.UpdatedOn,
                UpdatedById = x.UpdatedById,
                UpdatedByName = x.UpdatedByName,
                Reason= x.Reason == null ? null : new ReasonView
                {
                    Id = x.Reason.Id,
                    Name = x.Reason.NameEN,
                },
                TaskType = x.RefTaskType == null ? null : new RefTaskTypeView
                {
                    Id = x.RefTaskType.Id,
                    Name = x.RefTaskType.NameEN,
                },
                Note=x.Note

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
            //    Type = "getByCase",
            //    TableName = "casetasks",
            //    ActionType = "getByCase",
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
        [RequiredPermission("caseTasks.create")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAsync([FromBody] CaseTaskCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var caseTeamIds = unitOfWork.CaseTeamRepository
                .GetAllQuerable()
                .Where(e => e.CaseId == model.CaseId)
                .Select(e => e.UserId);

            if (!currentUser.IsSuperAdmin &&
                !await caseTeamIds.ContainsAsync(currentUser.UserId))
            {
                result.Status = (int)ResponseEnum.Unauthorized;
                result.Title = "Not authorized";

                return StatusCode(result.Status, result);
            }

            // Replace these values with your actual status IDs
            const int pendingStatusId = 1;
            const int compeletedStatusId = 2;

            var statusId = model.DueDate.Date > DateTime.Today
                ? pendingStatusId
                : compeletedStatusId;

            var entity = new TblCaseTask
            {
                CaseId = model.CaseId,
                Title = model.Title,
                DueDate = model.DueDate,
                PriorityId = model.PriorityId,
                StatusId = statusId,
                AssignedUserId = model.AssignedUserId,
                CreatedOn = DateTime.Now,
                CreatedById = currentUser.UserId,
                CreatedByName = currentUser.UserName,
                IsClosed = false,
                TaskTypeId = model.TaskTypeId,
                Note=model.Note
            };

            await unitOfWork.CaseTaskRepository.AddAsync(entity);

            await unitOfWork.CaseAuditLogRepository.AddAsync(
                new TblCaseAuditLog
                {
                    CaseId = model.CaseId,
                    Comment =
                        $"Case task created. Title: {model.Title}, " +
                        $"PriorityId: {model.PriorityId}, StatusId: {statusId}",
                    CreatedOn = DateTime.Now,
                    CreatedById = currentUser.UserId,
                    CreatedByName = currentUser.UserName
                });

            await unitOfWork.CompleteAsync();

            result.Data = entity.Id;

            var primaryKey = JsonSerializer.Serialize(new
            {
                Id = entity.Id
            });

            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "Create " + model.Title,
                TableName = "Case Tasks",
                ActionType = "Create",
                DateTime = DateTime.Now,
                OldValues = null,
                NewValues = null,
                AffectedColumns = null,
                PrimaryKey = primaryKey,
                IsArchived = false
            };

            await unitOfWork.AuditLogRepository.AddAsync(entityDocument);
            await unitOfWork.CompleteAsync();

            return StatusCode(result.Status, result);
        }


        //[HttpPost("create")]
        //[RequiredPermission("caseTasks.create")]
        //[ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> CreateAsync([FromBody] CaseTaskCommand model)
        //{
        //    var currentUser = await httpContext.GetCurrentUser();

        //    var result = new Response<long?>()
        //    {
        //        Data = null,
        //        Status = (int)ResponseEnum.Created,
        //        Title = "Created"
        //    };




        //    var caseTeamIds = unitOfWork.CaseTeamRepository.GetAllQuerable().Where(e => e.CaseId == model.CaseId).Select(e => e.UserId);
        //    if (!currentUser.IsSuperAdmin && !caseTeamIds.Contains(currentUser.UserId))
        //    {
        //        result.Status = (int)ResponseEnum.Unauthorized;
        //        result.Title = "Not authorized";
        //        return StatusCode(result.Status, result);
        //    }

        //    var entity = new TblCaseTask
        //    {
        //        CaseId = model.CaseId,
        //        Title = model.Title,
        //        DueDate = model.DueDate,
        //        PriorityId = model.PriorityId,
        //        StatusId = model.StatusId,
        //        AssignedUserId = model.AssignedUserId,
        //        CreatedOn = DateTime.Now,
        //        CreatedById = currentUser.UserId,
        //        CreatedByName = currentUser.UserName,
        //        IsClosed=false,
        //        TaskTypeId=model.TaskTypeId
        //    };

        //    await unitOfWork.CaseTaskRepository.AddAsync(entity);

        //    await unitOfWork.CaseAuditLogRepository.AddAsync(new TblCaseAuditLog
        //    {
        //        CaseId = model.CaseId,
        //        Comment = $"Case task created. Title: {model.Title}, PriorityId: {model.PriorityId}, StatusId: {model.StatusId}",
        //        CreatedOn = DateTime.Now,
        //        CreatedById = currentUser.UserId,
        //        CreatedByName = currentUser.UserName,
        //    });

        //    await unitOfWork.CompleteAsync();

        //    result.Data = entity.Id;

        //    var obj = new
        //    {
        //        Id = entity.Id
        //    };

        //    string json = JsonSerializer.Serialize(obj);
        //    var entityDocument = new TblAuditLog
        //    {
        //        UserId = currentUser.Email,
        //        Type = "Create"+ model.Title,
        //        TableName = "Case Tasks",
        //        ActionType = "Create",
        //        DateTime = DateTime.Now,
        //        OldValues = null,
        //        NewValues = null,
        //        AffectedColumns = null,
        //        PrimaryKey = json,
        //        IsArchived = false,

        //    };

        //    await unitOfWork.AuditLogRepository.AddAsync(entityDocument);

        //    await unitOfWork.CompleteAsync();


        //    return StatusCode(result.Status, result);
        //}

        // ========================= UPDATE =========================
        [HttpPost("{id}/update")]
        [RequiredPermission("caseTasks.update")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] CaseTaskUpdate model)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.CaseTaskRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);
            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var refs = await referancesService.GetAllReferancesAsync();
            var updatedOn = DateTime.Now;
            var auditLogEntities = new List<TblCaseAuditLog>();
            //var entityDocument = new TblAuditLog();
            var auditLogs = new List<TblAuditLog>();

            void AddAuditLog(string fieldName, object? oldValue, object? newValue)
            {
                auditLogEntities.Add(new TblCaseAuditLog
                {
                    CaseId = entity.CaseId,
                    Comment = $"Task {fieldName} updated from {oldValue ?? "NULL"} to {newValue ?? "NULL"}",
                    CreatedOn = updatedOn,
                    CreatedById = currentUser.UserId,
                    CreatedByName = currentUser.UserName,
                });


                //auditLogs.Add(new TblAuditLog
                //{
                //    UserId = currentUser.Email,
                //    Type = "update",
                //    TableName = "Case Tasks",
                //    ActionType = "update",
                //    DateTime = DateTime.Now,
                //    OldValues = oldValue.ToString(),
                //    NewValues = newValue.ToString(),
                //    AffectedColumns = fieldName,
                //    PrimaryKey = json,
                //    IsArchived = false,

                //}) ;

               

            }

            if (!string.IsNullOrWhiteSpace(model.Title) &&
                entity.Title != model.Title)
            {
                AddAuditLog("Title", entity.Title, model.Title);
                entity.Title = model.Title;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "Case Tasks",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.Title,
                    NewValues = model.Title,
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });

            }

            if (!string.IsNullOrWhiteSpace(model.Note) &&
               entity.Note != model.Note)
            {
                AddAuditLog("Note", entity.Title, model.Title);
                entity.Note = model.Note;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update Note",
                    TableName = "Case Tasks",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.Note,
                    NewValues = model.Note,
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });

            }

            if (model.DueDate.HasValue &&
                entity.DueDate.Date != model.DueDate.Value.Date)
            {
                AddAuditLog("DueDate", entity.DueDate, model.DueDate.Value);
                entity.DueDate = model.DueDate.Value;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "Case Tasks",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.DueDate.ToString(),
                    NewValues = model.DueDate.Value.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }

            if (model.PriorityId.HasValue &&
                entity.PriorityId != model.PriorityId.Value)
            {
                AddAuditLog("Priority", refs.TaskPriorities.FirstOrDefault(r => r.Id == entity.PriorityId)?.NameEN, refs.TaskPriorities.FirstOrDefault(r => r.Id == model.PriorityId.Value)?.NameEN);
                entity.PriorityId = model.PriorityId.Value;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "Case Tasks",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.PriorityId.ToString(),
                    NewValues = model.PriorityId.Value.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }

            if (model.TaskTypeId.HasValue &&
                 entity.TaskTypeId != model.TaskTypeId.Value)
            {
                //AddAuditLog("TaskTypeId", refs.task.FirstOrDefault(r => r.Id == entity.TaskTypeId)?.NameEN, refs.TaskPriorities.FirstOrDefault(r => r.Id == model.TaskTypeId.Value)?.NameEN);
                entity.TaskTypeId = model.TaskTypeId.Value;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "Case Tasks",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.TaskTypeId.ToString(),
                    NewValues = model.TaskTypeId.Value.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }

            if (model.AssignedUserId.HasValue &&
                entity.AssignedUserId != model.AssignedUserId.Value)
            {
                var affectedUserIds = new List<long>() { entity.AssignedUserId ?? 0, model.AssignedUserId.Value };
                var affectedUsers = await unitOfWork.UserRepository.GetAllQuerable().AsNoTracking().Where(e => affectedUserIds.Contains(e.Id)).ToListAsync();

                AddAuditLog("AssignedUser", affectedUsers.FirstOrDefault(u => u.Id == entity.AssignedUserId)?.NameEn, affectedUsers.FirstOrDefault(u => u.Id == model.AssignedUserId)?.NameEn);
                entity.AssignedUserId = model.AssignedUserId.Value;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "Case Tasks",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.AssignedUserId.ToString(),
                    NewValues = model.AssignedUserId.Value.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }

            entity.UpdatedOn = updatedOn;
            entity.UpdatedById = currentUser.UserId;
            entity.UpdatedByName = currentUser.UserName;

            unitOfWork.CaseTaskRepository.Update(entity);

            if (auditLogEntities.Any())
                await unitOfWork.CaseAuditLogRepository.AddRangeAsync(auditLogEntities);
            await unitOfWork.AuditLogRepository.AddRangeAsync(auditLogs);


            await unitOfWork.CompleteAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Updated";
            result.Data = true;

            return StatusCode(result.Status, result);
        }
        // ========================= Close Task =========================

        [HttpPost("{id}/Close")]
        [RequiredPermission("caseTasks.update")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CloseTaskAsync(long id, [FromBody] CaseTaskClose model)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.CaseTaskRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);
            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var auditLogs = new List<TblAuditLog>();
            var refs = await referancesService.GetAllReferancesAsync();
            var updatedOn = DateTime.Now;
            var auditLogEntities = new List<TblCaseAuditLog>();

            void AddAuditLog(string fieldName, object? oldValue, object? newValue)
            {
                auditLogEntities.Add(new TblCaseAuditLog
                {
                    CaseId = entity.CaseId,
                    Comment = $"Task {fieldName} updated from {oldValue ?? "NULL"} to {newValue ?? "NULL"}",
                    CreatedOn = updatedOn,
                    CreatedById = currentUser.UserId,
                    CreatedByName = currentUser.UserName,
                   
                });
            }

            var refTaskStatusEntity = await unitOfWork.RefTaskStatusRepository.GetAllQuerable()
               .FirstOrDefaultAsync(x => x.NameEN.Contains("Completed"));
                AddAuditLog("Status", refs.TaskStatuses.FirstOrDefault(r => r.Id == refTaskStatusEntity.Id)?.NameEN, refs.TaskStatuses.FirstOrDefault(r => r.Id == refTaskStatusEntity.Id)?.NameEN);
                entity.StatusId = refTaskStatusEntity.Id;
            
            auditLogs.Add(new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "close",
                TableName = "Case Tasks",
                ActionType = "close",
                DateTime = DateTime.Now,
                OldValues = "Pending",
                NewValues = "Closed",
                AffectedColumns = null,
                PrimaryKey = json,
                IsArchived = false,

            });



            entity.ReasonId = model.ReasonId;
            entity.IsClosed = true;
            entity.UpdatedOn = updatedOn;
            entity.UpdatedById = currentUser.UserId;
            entity.UpdatedByName = currentUser.UserName;

            unitOfWork.CaseTaskRepository.Update(entity);

            if (auditLogEntities.Any())
                await unitOfWork.CaseAuditLogRepository.AddRangeAsync(auditLogEntities);
            await unitOfWork.AuditLogRepository.AddRangeAsync(auditLogs);
            await unitOfWork.CompleteAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Updated";
            result.Data = true;


          
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "Close",
                TableName = "Case Tasks",
                ActionType = "Close",
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
        [RequiredPermission("caseTasks.delete")]
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

            var entity = await unitOfWork.CaseTaskRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            await unitOfWork.CaseAuditLogRepository.AddAsync(new TblCaseAuditLog
            {
                CaseId = entity.CaseId,
                Comment = $"Case task deleted. Id: {entity.Id}, Title: {entity.Title}, StatusId: {entity.StatusId}, PriorityId: {entity.PriorityId}",
                CreatedOn = DateTime.Now,
                CreatedById = currentUser.UserId,
                CreatedByName = currentUser.UserName,
            });

            unitOfWork.CaseTaskRepository.Delete(entity);

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
                TableName = "Case Tasks",
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



        //[HttpGet("{caseId}/ExportCaseTask")]
        //[RequiredPermission("caseTasks.getByCase")]
        //[ProducesResponseType(typeof(Response<IEnumerable<CaseTaskQuery>>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> ExportCaseAsync(long caseId)
        //{
        //    var currentUser = await httpContext.GetCurrentUser();
        //    var result = new Response<IEnumerable<CaseTaskQuery>?>()
        //    {
        //        Data = null,
        //        Status = (int)ResponseEnum.NotFound,
        //        Title = "Not Found"
        //    };

        //    var rows = unitOfWork.CaseTaskRepository.GetAllQuerable()
        //        .Include(e => e.AssignedUser).ThenInclude(e => e!.Type)
        //        .Include(e => e.Case).ThenInclude(e => e!.Court)
        //        .Include(e => e.Case).ThenInclude(e => e!.Team)
        //        .Include(e => e.Priority)
        //        .Include(e => e.Status)
        //        .Include(e => e.Reason)
        //        .Include(e => e.RefTaskType)
        //        .Where(e => e.CaseId == caseId)
        //        .Where(e => currentUser.IsSuperAdmin || e.Case!.Team.Select(t => t.UserId).Contains(currentUser.UserId))
        //        .AsNoTracking();



        //    // Projection
        //    var data = await rows.Select(x => new CaseTaskQuery
        //    {
        //        Id = x.Id,
        //        CaseId = x.CaseId,
        //        Title = x.Title,
        //        DueDate = x.DueDate,

        //        Priority = x.Priority == null ? null : new SummaryView
        //        {
        //            Id = x.Priority.Id,
        //            Name = x.Priority.NameEN,
        //        },

        //        Status = x.Status == null ? null : new SummaryView
        //        {
        //            Id = x.Status.Id,
        //            Name = x.Status.NameEN,
        //        },
        //        Court = x.Case!.Court == null ? null : new SummaryView
        //        {
        //            Id = x.Case.Court.Id,
        //            Name = x.Case.Court.NameEN,
        //        },

        //        User = x.AssignedUser == null ? null : new UserSummaryView
        //        {
        //            Id = x.AssignedUser.Id,
        //            NameEn = x.AssignedUser.NameEn,
        //            NameAr = x.AssignedUser.NameAr,
        //            Email = x.AssignedUser.Email,
        //            Type = x.AssignedUser.Type!.NameEN
        //        },
        //        IsClosed = x.IsClosed,
        //        CreatedOn = x.CreatedOn,
        //        CreatedById = x.CreatedById,
        //        CreatedByName = x.CreatedByName,

        //        UpdatedOn = x.UpdatedOn,
        //        UpdatedById = x.UpdatedById,
        //        UpdatedByName = x.UpdatedByName,
        //        Reason = x.Reason == null ? null : new ReasonView
        //        {
        //            Id = x.Reason.Id,
        //            Name = x.Reason.NameEN,
        //        },
        //        TaskType = x.RefTaskType == null ? null : new RefTaskTypeView
        //        {
        //            Id = x.RefTaskType.Id,
        //            Name = x.RefTaskType.NameEN,
        //        },

        //    }).ToListAsync();



        //    var templatePath = Path.Combine(
        //                 Directory.GetCurrentDirectory(),
        //                        "Templates",
        //                     "CaseTask.xlsx");

        //    string outputPath = Path.Combine(
        //        Path.GetTempPath(),
        //        $"CasesTask_{Guid.NewGuid()}.xlsx");

        //    System.IO.File.Copy(templatePath, outputPath, true);

        //    using (var workbook = new XLWorkbook(outputPath))
        //    {
        //        var worksheet = workbook.Worksheet(1);

        //        worksheet.Cell(2, 6).Value = DateTime.Now;
        //        worksheet.Cell(2, 6).Style.DateFormat.Format = "dd-MM-yyyy HH:mm";

        //        int row = 5;

        //        foreach (var c in data)
        //        {
        //            worksheet.Cell(row, 1).Value = c.Id;
        //            worksheet.Cell(row, 2).Value = c.Title;
        //            worksheet.Cell(row, 3).Value = c.DueDate;
        //            worksheet.Cell(row, 4).Value = c.Priority?.Name;
        //            worksheet.Cell(row, 5).Value = c.Status?.Name;
        //            worksheet.Cell(row, 6).Value = c.CreatedOn;
        //            row++;
        //        }

        //        worksheet.Columns().AdjustToContents();

        //        workbook.Save();
        //    }

        //    var fileBytes = await System.IO.File.ReadAllBytesAsync(outputPath);


        //    return PhysicalFile(outputPath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //                        $"CaseTask_{DateTime.Now:yyyyMMddHHmmss}.xlsx");


        //}

        [HttpGet("{caseId}/ExportCaseTask")]
        [RequiredPermission("caseTasks.getByCase")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportCaseAsync(long caseId)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var rows = unitOfWork.CaseTaskRepository
                .GetAllQuerable()
                .AsNoTracking()
                .Include(x=>x.Case)
                .ThenInclude(x=>x.AssignedUser)
                .Where(x => x.CaseId == caseId);

         

            var data = await rows
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new
                {
                    TaskId = x.Id,
                    x.CaseId,
                    Title = x.Title ?? string.Empty,
                    x.DueDate,

                    PriorityName = x.Priority != null
                        ? x.Priority.NameEN
                        : string.Empty,

                    StatusName = x.Status != null
                        ? x.Status.NameEN
                        : string.Empty,

                    CourtName =
                        x.Case != null && x.Case.Court != null
                            ? x.Case.Court.NameEN
                            : string.Empty,

                    AssignedUserNameEn = x.Case != null &&
                     x.Case.AssignedUser != null
    ? x.Case.AssignedUser.NameEn
    : string.Empty,

                    AssignedUserNameAr =
                        x.Case != null &&
                     x.Case.AssignedUser != null
    ? x.Case.AssignedUser.NameAr
    : string.Empty,

                    AssignedUserEmail =
                         x.Case != null &&
                     x.Case.AssignedUser != null
    ? x.Case.AssignedUser.Email
    : string.Empty,
                  
                })
                .ToListAsync();

            if (data.Count == 0)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = "No case tasks were found for this case."
                });
            }

            var templatePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Templates",
                "CaseTask.xlsx");

            if (!System.IO.File.Exists(templatePath))
            {
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Template not found",
                    detail: "The CaseTask.xlsx template file could not be found.");
            }

            var outputPath = Path.Combine(
                Path.GetTempPath(),
                $"CaseTask_{caseId}_{Guid.NewGuid():N}.xlsx");

            System.IO.File.Copy(
                templatePath,
                outputPath,
                overwrite: true);

            try
            {
                using var workbook = new XLWorkbook(outputPath);

                var worksheet = workbook.Worksheet(1);

                const int headerRow = 4;
                const int firstDataRow = 5;
                const int totalColumns = 10;

                // Report generated date
                worksheet.Cell(2, 6).Value = DateTime.Now;
                worksheet.Cell(2, 6).Style.DateFormat.Format =
                    "dd-MM-yyyy HH:mm";

                // Clear any previous template data
                var oldLastRow = worksheet.LastRowUsed()?.RowNumber() ?? firstDataRow;

                if (oldLastRow >= firstDataRow)
                {
                    worksheet.Range(
                            firstDataRow,
                            1,
                            oldLastRow,
                            totalColumns)
                        .Clear(XLClearOptions.Contents);
                }

                // Headers
                var headers = new[]
                {
            "Task ID",
            "Case ID",
            "Title",
            "Due Date",
            "Priority",
            "Status",
            "Court",
            "Assigned User English",
            "Assigned User Arabic",
            "User Email"
           
        };

                for (var column = 1; column <= headers.Length; column++)
                {
                    worksheet.Cell(headerRow, column).Value =
                        headers[column - 1];
                }

                var headerRange = worksheet.Range(
                    headerRow,
                    1,
                    headerRow,
                    totalColumns);

                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical =
                    XLAlignmentVerticalValues.Center;
                headerRange.Style.Alignment.WrapText = true;

                headerRange.Style.Border.OutsideBorder =
                    XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder =
                    XLBorderStyleValues.Thin;

                var row = firstDataRow;

                foreach (var task in data)
                {
                    worksheet.Cell(row, 1).Value = task.TaskId;
                    worksheet.Cell(row, 2).Value = task.CaseId;
                    worksheet.Cell(row, 3).Value = task.Title;

                    if (task.DueDate != DateTime.MinValue)
                    {
                        worksheet.Cell(row, 4).Value = task.DueDate;
                        worksheet.Cell(row, 4)
                            .Style.DateFormat.Format =
                            "dd-MM-yyyy HH:mm";
                    }
                    else
                    {
                        worksheet.Cell(row, 4).Value = string.Empty;
                    }

                    worksheet.Cell(row, 5).Value =
                        task.PriorityName;

                    worksheet.Cell(row, 6).Value =
                        task.StatusName;

                    worksheet.Cell(row, 7).Value =
                        task.CourtName;

                    worksheet.Cell(row, 8).Value =
                        task.AssignedUserNameEn;

                    worksheet.Cell(row, 9).Value =
                        task.AssignedUserNameAr;

                    worksheet.Cell(row, 10).Value =
                        task.AssignedUserEmail;

                   

                    row++;
                }

                var lastDataRow = row - 1;

                var tableRange = worksheet.Range(
                    headerRow,
                    1,
                    lastDataRow,
                    totalColumns);

                tableRange.Style.Border.OutsideBorder =
                    XLBorderStyleValues.Thin;

                tableRange.Style.Border.InsideBorder =
                    XLBorderStyleValues.Thin;

                tableRange.Style.Alignment.Vertical =
                    XLAlignmentVerticalValues.Center;

                worksheet.Range(
                        firstDataRow,
                        1,
                        lastDataRow,
                        totalColumns)
                    .Style.Alignment.WrapText = true;

                // Explicit date-column formatting
                worksheet.Range(
                        firstDataRow,
                        4,
                        lastDataRow,
                        4)
                    .Style.DateFormat.Format =
                    "dd-MM-yyyy HH:mm";

                // Better alignment
                worksheet.Range(
                        firstDataRow,
                        1,
                        lastDataRow,
                        2)
                    .Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                worksheet.Range(
                        firstDataRow,
                        4,
                        lastDataRow,
                        7)
                    .Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                worksheet.SheetView.FreezeRows(headerRow);

                worksheet.Columns(1, totalColumns)
                    .AdjustToContents();

                // Control column widths
                worksheet.Column(1).Width = 12;
                worksheet.Column(2).Width = 12;
                worksheet.Column(3).Width = 35;
                worksheet.Column(4).Width = 20;
                worksheet.Column(5).Width = 18;
                worksheet.Column(6).Width = 18;
                worksheet.Column(7).Width = 25;
                worksheet.Column(8).Width = 25;
                worksheet.Column(9).Width = 25;
                worksheet.Column(10).Width = 30;
                worksheet.Column(11).Width = 20;

                worksheet.Row(headerRow).Height = 30;

                workbook.SaveAs(outputPath);

                var fileBytes =
                    await System.IO.File.ReadAllBytesAsync(outputPath);

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"CaseTask_{caseId}_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            finally
            {
                if (System.IO.File.Exists(outputPath))
                {
                    System.IO.File.Delete(outputPath);
                }
            }
        }
    }
}
