using LegalSystem.Attributes;
using LegalSystem.DataAccess;
using LegalSystem.DTOs;
using LegalSystem.Enums;
using LegalSystem.Helpers;
using LegalSystem.Models;
using LegalSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LegalSystem.Controllers
{
    [Route("referances")]
    //[RequiredPermission("ref.managment")]
    [AllowAnonymous]
    [ApiController]
    public class ReferancesController : ControllerBase
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ReferancesService referancesService;
        private readonly HttpContextProvider httpContext;

        public ReferancesController(UnitOfWork unitOfWork, ReferancesService referancesService, HttpContextProvider httpContext)
        {
            this.unitOfWork = unitOfWork;
            this.referancesService = referancesService;
            this.httpContext = httpContext;
        }

        // ========================= GET ALL =========================
        [HttpGet]
        [RequiredPermission]
        [ProducesResponseType(typeof(Response<RefefancesQuery>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = new Response<RefefancesQuery>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var data = await referancesService.GetAllReferancesAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        // ========================= CASE TYPES =========================

        [HttpGet("case-types")]
        [RequiredPermission]
        [ProducesResponseType(typeof(Response<IEnumerable<BaseRefQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCaseTypesAsync([FromQuery] BaseRefFilter filter)
        {
            var result = new Response<IEnumerable<BaseRefQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.RefCaseTypeRepository.GetAllQuerable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                rows = rows.Where(e => (e.NameEN + e.NameAR).ToLower().Contains(filter.SearchText.ToLower()));

            if (filter.IsActive.HasValue)
                rows = rows.Where(e => e.IsActive == filter.IsActive.Value);

            var data = await rows.Select(e => new BaseRefQuery
            {
                Id = e.Id,
                NameEN = e.NameEN,
                NameAR = e.NameAR,
                Order = e.Order,
                IsActive = e.IsActive,
            }).ToListAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        [HttpPost("case-types/create")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCaseTypeAsync([FromBody] BaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var entity = new RefCaseType
            {
                NameEN = model.NameEN,
                NameAR = model.NameAR,
                Order = model.Order,
                IsActive = model.IsActive,
            };

            await unitOfWork.RefCaseTypeRepository.AddAsync(entity);
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
                TableName = "Case Types",
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

        [HttpPost("case-types/{id}/update")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCaseTypeAsync(long id, [FromBody] UpdateBaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.RefCaseTypeRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);


            entity.NameEN = string.IsNullOrWhiteSpace(model.NameEN) ? entity.NameEN : model.NameEN;
            entity.NameAR = string.IsNullOrWhiteSpace(model.NameAR) ? entity.NameAR : model.NameAR;
            entity.Order = model.Order.HasValue ? model.Order.Value : entity.Order;
            entity.IsActive = model.IsActive.HasValue ? model.IsActive.Value : entity.IsActive;


            unitOfWork.RefCaseTypeRepository.Update(entity);
            await unitOfWork.CompleteAsync();

            result.Data = true;
            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Created";
            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "update",
                TableName = "Case Types",
                ActionType = "update",
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
        // ========================= Reason =========================

        [HttpGet("reasons")]
        [RequiredPermission]
        [ProducesResponseType(typeof(Response<IEnumerable<BaseRefQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReasonsAsync([FromQuery] BaseRefFilter filter)
        {
            var result = new Response<IEnumerable<BaseReasonRefQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.ReasonRepository.GetAllQuerable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                rows = rows.Where(e => (e.NameEN + e.NameAR).ToLower().Contains(filter.SearchText.ToLower()));

            if (filter.IsActive.HasValue)
                rows = rows.Where(e => e.IsActive == filter.IsActive.Value);

            var data = await rows.Select(e => new BaseReasonRefQuery
            {
                Id = e.Id,
                NameEN = e.NameEN,
                NameAR = e.NameAR,
                Order = e.Order,
                IsActive = e.IsActive,
                ReasonType=e.ReasonType
                
            }).ToListAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        [HttpPost("reason/create")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateReasonAsync([FromBody] BaseReasonRefQuery model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var entity = new Reason
            {
                NameEN = model.NameEN,
                NameAR = model.NameAR,
                Order = model.Order,
                IsActive = model.IsActive,
                ReasonType=model.ReasonType
            };

            await unitOfWork.ReasonRepository.AddAsync(entity);
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
                TableName = "Reasons",
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

        [HttpPost("reason/{id}/update")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateReasonAsync(long id, [FromBody] UpdateReasonBaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.ReasonRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);


            entity.NameEN = string.IsNullOrWhiteSpace(model.NameEN) ? entity.NameEN : model.NameEN;
            entity.NameAR = string.IsNullOrWhiteSpace(model.NameAR) ? entity.NameAR : model.NameAR;
            entity.Order = model.Order.HasValue ? model.Order.Value : entity.Order;
            entity.IsActive = model.IsActive.HasValue ? model.IsActive.Value : entity.IsActive;
            entity.ReasonType = model.ReasonType.HasValue ? model.ReasonType.Value : entity.ReasonType;

            unitOfWork.ReasonRepository.Update(entity);
            await unitOfWork.CompleteAsync();

            result.Data = true;
            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Created";
            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "update",
                TableName = "Reasons",
                ActionType = "update",
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

        // ========================= CASE LEVELS =========================

        [HttpGet("case-levels")]
        [RequiredPermission]
        [ProducesResponseType(typeof(Response<IEnumerable<BaseRefQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCaseLevelsAsync([FromQuery] BaseRefFilter filter)
        {

            var result = new Response<IEnumerable<BaseRefQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.RefCaseLevelRepository.GetAllQuerable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                rows = rows.Where(e => (e.NameEN + e.NameAR).ToLower().Contains(filter.SearchText.ToLower()));

            if (filter.IsActive.HasValue)
                rows = rows.Where(e => e.IsActive == filter.IsActive.Value);

            var data = await rows.Select(e => new BaseRefQuery
            {
                Id = e.Id,
                NameEN = e.NameEN,
                NameAR = e.NameAR,
                Order = e.Order,
                IsActive = e.IsActive,
            }).ToListAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        [HttpPost("case-levels/create")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCaseLevelAsync([FromBody] BaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var entity = new RefCaseLevel
            {
                NameEN = model.NameEN,
                NameAR = model.NameAR,
                Order = model.Order,
                IsActive = model.IsActive,
            };

            await unitOfWork.RefCaseLevelRepository.AddAsync(entity);
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
                TableName = "Case Level",
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

        [HttpPost("case-levels/{id}/update")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCaseLevelAsync(long id, [FromBody] UpdateBaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.RefCaseLevelRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            entity.NameEN = string.IsNullOrWhiteSpace(model.NameEN) ? entity.NameEN : model.NameEN;
            entity.NameAR = string.IsNullOrWhiteSpace(model.NameAR) ? entity.NameAR : model.NameAR;
            entity.Order = model.Order.HasValue ? model.Order.Value : entity.Order;
            entity.IsActive = model.IsActive.HasValue ? model.IsActive.Value : entity.IsActive;

            unitOfWork.RefCaseLevelRepository.Update(entity);
            await unitOfWork.CompleteAsync();

            result.Data = true;
            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Created";

            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "update",
                TableName = "Case Level",
                ActionType = "update",
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

        // ========================= CASE STATUS =========================

        [HttpGet("case-statuses")]
        [RequiredPermission]
        [ProducesResponseType(typeof(Response<IEnumerable<BaseRefQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCaseStatusesAsync([FromQuery] BaseRefFilter filter)
        {
            var result = new Response<IEnumerable<BaseRefQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.RefCaseStatusRepository.GetAllQuerable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                rows = rows.Where(e => (e.NameEN + e.NameAR).ToLower().Contains(filter.SearchText.ToLower()));

            if (filter.IsActive.HasValue)
                rows = rows.Where(e => e.IsActive == filter.IsActive.Value);

            var data = await rows.Select(e => new BaseRefQuery
            {
                Id = e.Id,
                NameEN = e.NameEN,
                NameAR = e.NameAR,
                Order = e.Order,
                IsActive = e.IsActive,
            }).ToListAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        [HttpPost("case-statuses/create")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCaseStatusAsync([FromBody] BaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var entity = new RefCaseStatus
            {
                NameEN = model.NameEN,
                NameAR = model.NameAR,
                Order = model.Order,
                IsActive = model.IsActive,
            };

            await unitOfWork.RefCaseStatusRepository.AddAsync(entity);
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
                TableName = "Case Status",
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

        [HttpPost("case-statuses/{id}/update")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCaseStatusAsync(long id, [FromBody] UpdateBaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.RefCaseStatusRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            entity.NameEN = string.IsNullOrWhiteSpace(model.NameEN) ? entity.NameEN : model.NameEN;
            entity.NameAR = string.IsNullOrWhiteSpace(model.NameAR) ? entity.NameAR : model.NameAR;
            entity.Order = model.Order.HasValue ? model.Order.Value : entity.Order;
            entity.IsActive = model.IsActive.HasValue ? model.IsActive.Value : entity.IsActive;

            unitOfWork.RefCaseStatusRepository.Update(entity);
            await unitOfWork.CompleteAsync();

            result.Data = true;
            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Created";

            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "update",
                TableName = "Case Status",
                ActionType = "update",
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

        // ========================= COURTS =========================

        [HttpGet("courts")]
        [RequiredPermission]
        [ProducesResponseType(typeof(Response<IEnumerable<BaseRefQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCourtsAsync([FromQuery] BaseRefFilter filter)
        {
            var result = new Response<IEnumerable<BaseRefQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.RefCourtRepository.GetAllQuerable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                rows = rows.Where(e => (e.NameEN + e.NameAR).ToLower().Contains(filter.SearchText.ToLower()));

            if (filter.IsActive.HasValue)
                rows = rows.Where(e => e.IsActive == filter.IsActive.Value);

            var data = await rows.Select(e => new BaseRefQuery
            {
                Id = e.Id,
                NameEN = e.NameEN,
                NameAR = e.NameAR,
                Order = e.Order,
                IsActive = e.IsActive,
            }).ToListAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        [HttpPost("courts/create")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCourtAsync([FromBody] BaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var entity = new RefCourt
            {
                NameEN = model.NameEN,
                NameAR = model.NameAR,
                Order = model.Order,
                IsActive = model.IsActive,
            };

            await unitOfWork.RefCourtRepository.AddAsync(entity);
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
                TableName = "Courts",
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

        [HttpPost("courts/{id}/update")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCourtAsync(long id, [FromBody] UpdateBaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.RefCourtRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            entity.NameEN = string.IsNullOrWhiteSpace(model.NameEN) ? entity.NameEN : model.NameEN;
            entity.NameAR = string.IsNullOrWhiteSpace(model.NameAR) ? entity.NameAR : model.NameAR;
            entity.Order = model.Order.HasValue ? model.Order.Value : entity.Order;
            entity.IsActive = model.IsActive.HasValue ? model.IsActive.Value : entity.IsActive;

            unitOfWork.RefCourtRepository.Update(entity);
            await unitOfWork.CompleteAsync();

            result.Data = true;
            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Created";
            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "update",
                TableName = "Courts",
                ActionType = "update",
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

        // ========================= RELIEF SOUGHT =========================

        [HttpGet("relief-sought")]
        [RequiredPermission]
        [ProducesResponseType(typeof(Response<IEnumerable<BaseRefQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReliefSoughtAsync([FromQuery] BaseRefFilter filter)
        {
            var result = new Response<IEnumerable<BaseRefQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.RefReliefSoughtRepository.GetAllQuerable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                rows = rows.Where(e => (e.NameEN + e.NameAR).ToLower().Contains(filter.SearchText.ToLower()));

            if (filter.IsActive.HasValue)
                rows = rows.Where(e => e.IsActive == filter.IsActive.Value);

            var data = await rows.Select(e => new BaseRefQuery
            {
                Id = e.Id,
                NameEN = e.NameEN,
                NameAR = e.NameAR,
                Order = e.Order,
                IsActive = e.IsActive,
            }).ToListAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        [HttpPost("relief-sought/create")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateReliefSoughtAsync([FromBody] BaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var entity = new RefReliefSought
            {
                NameEN = model.NameEN,
                NameAR = model.NameAR,
                Order = model.Order,
                IsActive = model.IsActive,
            };

            await unitOfWork.RefReliefSoughtRepository.AddAsync(entity);
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
                TableName = "Relief sought",
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

        [HttpPost("relief-sought/{id}/update")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateReliefSoughtAsync(long id, [FromBody] UpdateBaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.RefReliefSoughtRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            entity.NameEN = string.IsNullOrWhiteSpace(model.NameEN) ? entity.NameEN : model.NameEN;
            entity.NameAR = string.IsNullOrWhiteSpace(model.NameAR) ? entity.NameAR : model.NameAR;
            entity.Order = model.Order.HasValue ? model.Order.Value : entity.Order;
            entity.IsActive = model.IsActive.HasValue ? model.IsActive.Value : entity.IsActive;

            unitOfWork.RefReliefSoughtRepository.Update(entity);
            await unitOfWork.CompleteAsync();

            result.Data = true;
            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Created";
            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "update",
                TableName = "Relief sought",
                ActionType = "update",
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

        // ========================= USER TYPES =========================

        [HttpGet("user-types")]
        [RequiredPermission]
        [ProducesResponseType(typeof(Response<IEnumerable<BaseRefQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserTypesAsync([FromQuery] BaseRefFilter filter)
        {
            var result = new Response<IEnumerable<BaseRefQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.RefUserTypeRepository.GetAllQuerable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                rows = rows.Where(e => (e.NameEN + e.NameAR).ToLower().Contains(filter.SearchText.ToLower()));

            if (filter.IsActive.HasValue)
                rows = rows.Where(e => e.IsActive == filter.IsActive.Value);

            var data = await rows.Select(e => new BaseRefQuery
            {
                Id = e.Id,
                NameEN = e.NameEN,
                NameAR = e.NameAR,
                Order = e.Order,
                IsActive = e.IsActive,
            }).ToListAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        [HttpPost("user-types/create")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUserTypeAsync([FromBody] BaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var entity = new RefUserType
            {
                NameEN = model.NameEN,
                NameAR = model.NameAR,
                Order = model.Order,
                IsActive = model.IsActive,
            };

            await unitOfWork.RefUserTypeRepository.AddAsync(entity);
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
                TableName = "User Type",
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

        [HttpPost("user-types/{id}/update")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserTypeAsync(long id, [FromBody] UpdateBaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.RefUserTypeRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            entity.NameEN = string.IsNullOrWhiteSpace(model.NameEN) ? entity.NameEN : model.NameEN;
            entity.NameAR = string.IsNullOrWhiteSpace(model.NameAR) ? entity.NameAR : model.NameAR;
            entity.Order = model.Order.HasValue ? model.Order.Value : entity.Order;
            entity.IsActive = model.IsActive.HasValue ? model.IsActive.Value : entity.IsActive;

            unitOfWork.RefUserTypeRepository.Update(entity);
            await unitOfWork.CompleteAsync();

            result.Data = true;
            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Created";

            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "update",
                TableName = "User Type",
                ActionType = "update",
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

        // ========================= TASK PRIORITY =========================
        [HttpGet("task-priorities")]
        [RequiredPermission]
        [ProducesResponseType(typeof(Response<IEnumerable<BaseRefQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTaskPrioritiesAsync([FromQuery] BaseRefFilter filter)
        {

            var result = new Response<IEnumerable<BaseRefQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.RefTaskPriorityRepository.GetAllQuerable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                rows = rows.Where(e => (e.NameEN + e.NameAR)
                    .ToLower()
                    .Contains(filter.SearchText.ToLower()));

            if (filter.IsActive.HasValue)
                rows = rows.Where(e => e.IsActive == filter.IsActive.Value);

            var data = await rows.Select(e => new BaseRefQuery
            {
                Id = e.Id,
                NameEN = e.NameEN,
                NameAR = e.NameAR,
                Order = e.Order,
                IsActive = e.IsActive,
            }).ToListAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        [HttpPost("task-priorities/create")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTaskPriorityAsync([FromBody] BaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var entity = new RefTaskPriority
            {
                NameEN = model.NameEN,
                NameAR = model.NameAR,
                Order = model.Order,
                IsActive = model.IsActive,
            };

            await unitOfWork.RefTaskPriorityRepository.AddAsync(entity);
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
                TableName = "Task Priority",
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

        [HttpPost("task-priorities/{id}/update")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTaskPriorityAsync(long id, [FromBody] UpdateBaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.RefTaskPriorityRepository
                .GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            entity.NameEN = string.IsNullOrWhiteSpace(model.NameEN) ? entity.NameEN : model.NameEN;
            entity.NameAR = string.IsNullOrWhiteSpace(model.NameAR) ? entity.NameAR : model.NameAR;
            entity.Order = model.Order.HasValue ? model.Order.Value : entity.Order;
            entity.IsActive = model.IsActive.HasValue ? model.IsActive.Value : entity.IsActive;

            unitOfWork.RefTaskPriorityRepository.Update(entity);
            await unitOfWork.CompleteAsync();

            result.Data = true;
            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Updated";
            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "update",
                TableName = "Task Priority",
                ActionType = "update",
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

        // ========================= TASK STATUS =========================
        [HttpGet("task-statuses")]
        [RequiredPermission]
        [ProducesResponseType(typeof(Response<IEnumerable<BaseRefQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTaskStatusesAsync([FromQuery] BaseRefFilter filter)
        {
            var result = new Response<IEnumerable<BaseRefQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.RefTaskStatusRepository.GetAllQuerable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                rows = rows.Where(e =>
                    (e.NameEN + e.NameAR).ToLower().Contains(filter.SearchText.ToLower()));

            if (filter.IsActive.HasValue)
                rows = rows.Where(e => e.IsActive == filter.IsActive.Value);

            var data = await rows.Select(e => new BaseRefQuery
            {
                Id = e.Id,
                NameEN = e.NameEN,
                NameAR = e.NameAR,
                Order = e.Order,
                IsActive = e.IsActive,
            }).ToListAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        [HttpPost("task-statuses/create")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTaskStatusAsync([FromBody] BaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var entity = new RefTaskStatus
            {
                NameEN = model.NameEN,
                NameAR = model.NameAR,
                Order = model.Order,
                IsActive = model.IsActive,
            };

            await unitOfWork.RefTaskStatusRepository.AddAsync(entity);
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
                TableName = "Task Status",
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

        [HttpPost("task-statuses/{id}/update")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTaskStatusAsync(long id, [FromBody] UpdateBaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.RefTaskStatusRepository
                .GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            entity.NameEN = string.IsNullOrWhiteSpace(model.NameEN) ? entity.NameEN : model.NameEN;
            entity.NameAR = string.IsNullOrWhiteSpace(model.NameAR) ? entity.NameAR : model.NameAR;
            entity.Order = model.Order.HasValue ? model.Order.Value : entity.Order;
            entity.IsActive = model.IsActive.HasValue ? model.IsActive.Value : entity.IsActive;

            unitOfWork.RefTaskStatusRepository.Update(entity);
            await unitOfWork.CompleteAsync();

            result.Data = true;
            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Updated";

            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "update",
                TableName = "Task Status",
                ActionType = "update",
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

        // ========================= DOCUMENT CLASSIFICATIONS =========================
        [HttpGet("document-classifications")]
        [RequiredPermission]
        [ProducesResponseType(typeof(Response<IEnumerable<BaseRefQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDocumentClassificationsAsync([FromQuery] BaseRefFilter filter)
        {
            var result = new Response<IEnumerable<BaseRefQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.RefDocumentClassificationRepository.GetAllQuerable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                rows = rows.Where(e =>
                    (e.NameEN + e.NameAR).ToLower().Contains(filter.SearchText.ToLower()));

            if (filter.IsActive.HasValue)
                rows = rows.Where(e => e.IsActive == filter.IsActive.Value);

            var data = await rows.Select(e => new BaseRefQuery
            {
                Id = e.Id,
                NameEN = e.NameEN,
                NameAR = e.NameAR,
                Order = e.Order,
                IsActive = e.IsActive,
            }).ToListAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        [HttpPost("document-classifications/create")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateDocumentClassificationAsync([FromBody] BaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var entity = new RefDocumentClassification
            {
                NameEN = model.NameEN,
                NameAR = model.NameAR,
                Order = model.Order,
                IsActive = model.IsActive,
            };

            await unitOfWork.RefDocumentClassificationRepository.AddAsync(entity);
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
                TableName = "Document Classification",
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

        [HttpPost("document-classifications/{id}/update")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateDocumentClassificationAsync(long id, [FromBody] UpdateBaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.RefDocumentClassificationRepository
                .GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            entity.NameEN = string.IsNullOrWhiteSpace(model.NameEN) ? entity.NameEN : model.NameEN;
            entity.NameAR = string.IsNullOrWhiteSpace(model.NameAR) ? entity.NameAR : model.NameAR;
            entity.Order = model.Order.HasValue ? model.Order.Value : entity.Order;
            entity.IsActive = model.IsActive.HasValue ? model.IsActive.Value : entity.IsActive;

            unitOfWork.RefDocumentClassificationRepository.Update(entity);
            await unitOfWork.CompleteAsync();

            result.Data = true;
            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Updated";

            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "update",
                TableName = "Document Classification",
                ActionType = "update",
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

        // ========================= EVENT TYPE =========================
        [HttpGet("event-types")]
        [RequiredPermission]
        [ProducesResponseType(typeof(Response<IEnumerable<BaseRefQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEventTypesAsync([FromQuery] BaseRefFilter filter)
        {
            var result = new Response<IEnumerable<BaseRefQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.RefEventTypeRepository.GetAllQuerable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                rows = rows.Where(e =>
                    (e.NameEN + e.NameAR).ToLower().Contains(filter.SearchText.ToLower()));

            if (filter.IsActive.HasValue)
                rows = rows.Where(e => e.IsActive == filter.IsActive.Value);

            var data = await rows.Select(e => new BaseRefQuery
            {
                Id = e.Id,
                NameEN = e.NameEN,
                NameAR = e.NameAR,
                Order = e.Order,
                IsActive = e.IsActive,
            }).ToListAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        [HttpPost("event-types/create")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateEventTypeAsync([FromBody] BaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var entity = new RefEventType
            {
                NameEN = model.NameEN,
                NameAR = model.NameAR,
                Order = model.Order,
                IsActive = model.IsActive,
            };

            await unitOfWork.RefEventTypeRepository.AddAsync(entity);
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
                TableName = "Event Type",
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

        [HttpPost("event-types/{id}/update")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateEventTypeAsync(long id, [FromBody] UpdateBaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.RefEventTypeRepository
                .GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            entity.NameEN = string.IsNullOrWhiteSpace(model.NameEN) ? entity.NameEN : model.NameEN;
            entity.NameAR = string.IsNullOrWhiteSpace(model.NameAR) ? entity.NameAR : model.NameAR;
            entity.Order = model.Order.HasValue ? model.Order.Value : entity.Order;
            entity.IsActive = model.IsActive.HasValue ? model.IsActive.Value : entity.IsActive;

            unitOfWork.RefEventTypeRepository.Update(entity);
            await unitOfWork.CompleteAsync();

            result.Data = true;
            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Updated";
            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "update",
                TableName = "Event Type",
                ActionType = "update",
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

        // ========================= Ref task Type =========================

        [HttpGet("task-type")]
        [RequiredPermission]
        [ProducesResponseType(typeof(Response<IEnumerable<BaseRefQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTaskTypeAsync([FromQuery] BaseRefFilter filter)
        {
            var result = new Response<IEnumerable<BaseRefQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.RefTaskTypeRepository.GetAllQuerable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                rows = rows.Where(e => (e.NameEN + e.NameAR).ToLower().Contains(filter.SearchText.ToLower()));

            if (filter.IsActive.HasValue)
                rows = rows.Where(e => e.IsActive == filter.IsActive.Value);

            var data = await rows.Select(e => new BaseRefQuery
            {
                Id = e.Id,
                NameEN = e.NameEN,
                NameAR = e.NameAR,
                Order = e.Order,
                IsActive = e.IsActive,
            }).ToListAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        [HttpPost("task-type/create")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTaskTypeAsync([FromBody] BaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var entity = new RefTaskType
            {
                NameEN = model.NameEN,
                NameAR = model.NameAR,
                Order = model.Order,
                IsActive = model.IsActive,
            };

            await unitOfWork.RefTaskTypeRepository.AddAsync(entity);
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
                TableName = "Task Type",
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

        [HttpPost("task-type/{id}/update")]
        [RequiredPermission("modify.refs")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTaskTypeAsync(long id, [FromBody] UpdateBaseRefCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.RefTaskTypeRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            entity.NameEN = string.IsNullOrWhiteSpace(model.NameEN) ? entity.NameEN : model.NameEN;
            entity.NameAR = string.IsNullOrWhiteSpace(model.NameAR) ? entity.NameAR : model.NameAR;
            entity.Order = model.Order.HasValue ? model.Order.Value : entity.Order;
            entity.IsActive = model.IsActive.HasValue ? model.IsActive.Value : entity.IsActive;

            unitOfWork.RefTaskTypeRepository.Update(entity);
            await unitOfWork.CompleteAsync();

            result.Data = true;
            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Created";

            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "update",
                TableName = "Task Type",
                ActionType = "update",
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
