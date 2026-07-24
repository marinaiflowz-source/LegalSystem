using LegalSystem.Attributes;
using LegalSystem.Constants;
using LegalSystem.DataAccess;
using LegalSystem.DTOs;
using LegalSystem.Enums;
using LegalSystem.Helpers;
using LegalSystem.Models;
using LegalSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LegalSystem.Controllers
{
    [Route("users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UnitOfWork unitOfWork;
        private readonly IdentityManagmentService identityManagmentService;
        private readonly HttpContextProvider httpContext;

        public UsersController(UnitOfWork unitOfWork, IdentityManagmentService identityManagmentService, HttpContextProvider httpContext)
        {
            this.unitOfWork = unitOfWork;
            this.identityManagmentService = identityManagmentService;
            this.httpContext = httpContext;
        }


        // ========================= GET ALL =========================
        [HttpGet]
        [RequiredPermission("users.get")]
        [ProducesResponseType(typeof(QueryResult<UserQuery>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync([FromQuery] UserFilter filter)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new QueryResult<UserQuery>()
            {
                Status = (int)ResponseEnum.Succeeded,
                Title = "Data Retrieved"
            };

            var rows = unitOfWork.UserRepository.GetAllQuerable()
                .AsNoTracking();

            // Filtering
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                rows = rows.Where(x =>
                    (x.NameEn + x.NameAr).Contains(filter.SearchText));
            }

            if (filter.TypeId.HasValue)
                rows = rows.Where(x => x.TypeId == filter.TypeId.Value);

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
                "NameEn" => filter.IsAscending ? rows.OrderBy(x => x.NameEn) : rows.OrderByDescending(x => x.NameEn),
                "NameAr" => filter.IsAscending ? rows.OrderBy(x => x.NameAr) : rows.OrderByDescending(x => x.NameAr),
                "CreatedOn" => filter.IsAscending ? rows.OrderBy(x => x.CreatedOn) : rows.OrderByDescending(x => x.CreatedOn),
                _ => filter.IsAscending ? rows.OrderBy(x => x.Id) : rows.OrderByDescending(x => x.Id),
            };

            result.Data.Count = totalItems;

            if (filter.Size > 0)
            {
                rows = rows.Skip(filter.Index * filter.Size).Take(filter.Size);
            }

            var data = await rows.Select(x => new UserQuery
            {
                Id = x.Id,
                RefId = x.RefId,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                TypeId = x.TypeId,
                IsActive = x.IsActive,
                CreatedOn = x.CreatedOn,
                CreatedById = x.CreatedById,
                CreatedByName = x.CreatedByName,
                UpdatedOn = x.UpdatedOn,
                UpdatedById = x.UpdatedById,
                UpdatedByName = x.UpdatedByName
            }).ToListAsync();

            result.Data.Rows = data;

            //var entityDocument = new TblAuditLog
            //{
            //    UserId = currentUser.Email,
            //    Type = "GetAll",
            //    TableName = "Users",
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

        // ========================= GET BY ID =========================
        [HttpGet("{id}")]
        [RequiredPermission("users.details")]
        [ProducesResponseType(typeof(Response<UserQuery>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<UserQuery?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var data = await unitOfWork.UserRepository.GetAllQuerable()
                .Where(x => x.Id == id)
                .AsNoTracking()
                .Select(x => new UserQuery
                {
                    Id = x.Id,
                    RefId = x.RefId,
                    NameEn = x.NameEn,
                    NameAr = x.NameAr,
                    TypeId = x.TypeId,
                    IsActive = x.IsActive,
                    CreatedOn = x.CreatedOn,
                    CreatedById = x.CreatedById,
                    CreatedByName = x.CreatedByName,
                    UpdatedOn = x.UpdatedOn,
                    UpdatedById = x.UpdatedById,
                    UpdatedByName = x.UpdatedByName
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return StatusCode(result.Status, result);

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            //var obj = new
            //{
            //    Id = id
            //};

            //string json = JsonSerializer.Serialize(obj);
            //var entityDocument = new TblAuditLog
            //{
            //    UserId = currentUser.Email,
            //    Type = "GetById",
            //    TableName = "Users",
            //    ActionType = "GetById",
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

        // ========================= CREATE =========================
        [HttpPost("create")]
        [RequiredPermission("users.create")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAsync([FromBody] UserCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Failed,
                Title = "Failed"
            };

            if (model.TypeId == 1)
            {
                var LegalAdvisorUser = await unitOfWork.UserRepository.GetAllQuerable().AsNoTracking().FirstOrDefaultAsync(e => e.TypeId == 1);
                if (LegalAdvisorUser is not null)
                {
                    result.Status = (int)ResponseEnum.Conflict;
                    result.Title = "You can not create more than one legal manager";
                    return StatusCode(result.Status, result);
                }
            }
            if (!currentUser.IsSuperAdmin && model.RefId.HasValue)
            {
                result.Status = (int)ResponseEnum.Unauthorized;
                result.Title = "Direct linking is allowed only for the admin.";
                return StatusCode(result.Status, result);
            }

            // Add in identity
            if (!model.RefId.HasValue)
            {
                var identityModel = new AddIdentityUserCommand()
                {
                    NameEn = model.NameEn,
                    NameAr = model.NameAr,
                    Email = model.Email,
                    CompanyId = 4,
                    PhoneNumber = model.PhoneNumber,
                    CountryCode = "971",
                    UserTypeId = 888,
                    Password = model.Password,
                    ManagerId = model.ManagerRefId,
                };
                var identityRes = await identityManagmentService.RegisterUser(identityModel);

                if (identityRes is not null && identityRes.Status == HttpResponsesEnum.Succeeded && identityRes.Data.UserId > 0)
                    model.RefId = identityRes.Data.UserId;
            }

            if (model.RefId.HasValue)
            {
                // persist
                var entity = new TblUser
                {
                    NameEn = model.NameEn,
                    NameAr = model.NameAr,
                    RefId = model.RefId.Value,
                    Email = model.Email,
                    TypeId = model.TypeId,

                    IsActive = true,
                    CreatedOn = DateTime.Now,
                    CreatedById = currentUser.UserId,
                    CreatedByName = currentUser.UserName
                };

                await unitOfWork.UserRepository.AddAsync(entity);
                await unitOfWork.CompleteAsync();

                result.Data = entity.Id;
                result.Status = (int)ResponseEnum.Created;
                result.Title = "Created";

                var obj = new
                {
                    Id = entity.Id
                };

                string json = JsonSerializer.Serialize(obj);
                var entityDocument = new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "Create",
                    TableName = "Users",
                    ActionType = "Create",
                    DateTime = DateTime.Now,
                    OldValues = null,
                    NewValues = null,
                    AffectedColumns = null,
                    PrimaryKey = null,
                    IsArchived = false,

                };

                await unitOfWork.AuditLogRepository.AddAsync(entityDocument);

                await unitOfWork.CompleteAsync();
            }

           
            return StatusCode(result.Status, result);
        }

        // ========================= UPDATE =========================
        [HttpPost("{id}/update")]
        [RequiredPermission("users.update")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateUserCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.UserRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            entity.NameEn = string.IsNullOrWhiteSpace(model.NameEn) ? entity.NameEn : model.NameEn;
            entity.NameAr = string.IsNullOrWhiteSpace(model.NameAr) ? entity.NameAr : model.NameAr;
            entity.RefId = model.RefId.HasValue ? model.RefId.Value : entity.RefId;
            entity.TypeId = model.TypeId.HasValue ? model.TypeId.Value : entity.TypeId;
            entity.IsActive = model.IsActive.HasValue ? model.IsActive.Value : entity.IsActive;

            entity.UpdatedOn = DateTime.Now;
            entity.UpdatedById = currentUser.UserId;
            entity.UpdatedByName = currentUser.UserName;

            unitOfWork.UserRepository.Update(entity);
            await unitOfWork.CompleteAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Updated";
            result.Data = true;

            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "update",
                TableName = "Users",
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


        [HttpGet("GetList")]
        [RequiredPermission("cases.get")]
        public async Task<IActionResult> GetListAsync()
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new QueryResult<UserQuery>()
            {
                Status = (int)ResponseEnum.Succeeded,
                Title = "Data Retrieved"
            };

            var rows = unitOfWork.UserRepository.GetAllQuerable()
                //.Where(e => currentUser.IsSuperAdmin || e.Team.Select(t => t.UserId).Contains(currentUser.UserId))
                .AsNoTracking();




            // Projection
            var data = await rows.Select(x => new
            {
                user = x,
            }).ToListAsync();

            result.Data.Rows = data.Select(x =>
            {

                return new UserQuery
                {
                    Id = x.user.Id,
                    NameEn = x.user.NameEn,
                };
            }).ToList();


            return StatusCode(result.Status, result);
        }

    }
}

