using LegalSystem.Attributes;
using LegalSystem.DataAccess;
using LegalSystem.DTOs;
using LegalSystem.Enums;
using LegalSystem.Helpers;
using LegalSystem.Models;
using LegalSystem.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ClosedXML.Excel;

namespace LegalSystem.Controllers
{
    [Route("cases")]
    [ApiController]
    public class CasesController : ControllerBase
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ReferancesService referancesService;
        private readonly HttpContextProvider httpContext;
        private readonly CrmService crmService;
        private readonly IWebHostEnvironment _env;
        public CasesController(UnitOfWork unitOfWork, HttpContextProvider httpContext, ReferancesService referancesService, CrmService crmService)
        {
            this.unitOfWork = unitOfWork;
            this.httpContext = httpContext;
            this.referancesService = referancesService;
            this.crmService = crmService;
        }

        // ========================= GET ALL =========================
        [HttpGet]
        [RequiredPermission("cases.get")]
        [ProducesResponseType(typeof(QueryResult<CaseQuery>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync([FromQuery] CaseFilter filter)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new QueryResult<CaseQuery>()
            {
                Status = (int)ResponseEnum.Succeeded,
                Title = "Data Retrieved"
            };

            var rows = unitOfWork.CaseRepository.GetAllQuerable()
                .Include(e => e.Type)
                .Include(e => e.Level)
                .Include(e => e.Court)
                .Include(e => e.Status)
                .Include(e => e.ReliefSought)
                .Include(e => e.Team)
                .Include(x => x.MainCase)
                .Where(e => currentUser.IsSuperAdmin || e.CreatedById == currentUser.UserId)
                .AsNoTracking();

            // Filtering
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                rows = rows.Where(x =>
                    (x.Claimant + x.Defendant + (x.UnitCode ?? "")).Contains(filter.SearchText));
            }

            if (filter.TypeId.HasValue)
                rows = rows.Where(x => x.TypeId == filter.TypeId.Value);

            if (filter.LevelId.HasValue)
                rows = rows.Where(x => x.LevelId == filter.LevelId.Value);

            if (filter.CourtId.HasValue)
                rows = rows.Where(x => x.CourtId == filter.CourtId.Value);

            if (filter.StatusId.HasValue)
                rows = rows.Where(x => x.StatusId == filter.StatusId.Value);

            if (filter.ReliefSoughtId.HasValue)
                rows = rows.Where(x => x.ReliefSoughtId == filter.ReliefSoughtId.Value);
            if (!string.IsNullOrEmpty(filter.LeadID))
                rows = rows.Where(x => x.LeadID.Contains(filter.LeadID));

            if (filter.MainCaseId.HasValue)
                rows = rows.Where(x => x.MainCaseId == filter.MainCaseId.Value);

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
                "Claimant" => filter.IsAscending ? rows.OrderBy(x => x.Claimant) : rows.OrderByDescending(x => x.Claimant),
                "Defendant" => filter.IsAscending ? rows.OrderBy(x => x.Defendant) : rows.OrderByDescending(x => x.Defendant),
                "CreatedOn" => filter.IsAscending ? rows.OrderBy(x => x.CreatedOn) : rows.OrderByDescending(x => x.CreatedOn),
                _ => filter.IsAscending ? rows.OrderBy(x => x.Id) : rows.OrderByDescending(x => x.Id),
            };

            // Paging
            result.Data.Count = totalItems;

            if (filter.Size > 0)
            {
                rows = rows.Skip(filter.Index * filter.Size).Take(filter.Size);
            }

            // Load workflow steps
            //var workflows = await unitOfWork.WorkflowRepository
            //    .GetAllQuerable()
            //    .Include(x => x.CaseStatus)
            //    .ToListAsync();

            // Projection
            var data = await rows.Select(x => new
            {
                Case = x,
            }).ToListAsync();

            result.Data.Rows = data.Select(x =>
            {
                //var currentWorkflow = workflows
                //    .Where(w => w.CaseTypeId == x.Case.TypeId)
                //    .OrderBy(w => w.Order)
                //    .ToList();

                //var currentStep = currentWorkflow
                //    .FirstOrDefault(w => w.CaseStatusId == x.Case.StatusId);

                //var nextStep = currentStep == null
                //    ? null
                //    : currentWorkflow.FirstOrDefault(w => w.Order == currentStep.Order + 1);

                return new CaseQuery
                {
                    Id = x.Case.Id,
                    Claimant = x.Case.Claimant,
                    Defendant = x.Case.Defendant,
                    ClaimValue = x.Case.ClaimValue,
                    Summary = x.Case.Summary,
                    UnitCode = x.Case.UnitCode,

                    Type = new SummaryView
                    {
                        Id = x.Case.Type!.Id,
                        Name = x.Case.Type.NameEN
                    },

                    Level = new SummaryView
                    {
                        Id = x.Case.Level!.Id,
                        Name = x.Case.Level.NameEN
                    },

                    Court = new SummaryView
                    {
                        Id = x.Case.Court!.Id,
                        Name = x.Case.Court.NameEN
                    },

                    Status = new SummaryView
                    {
                        Id = x.Case.Status!.Id,
                        Name = x.Case.Status.NameEN
                    },

                    //NextStatus = nextStep != null
                    //    ? new SummaryView
                    //    {
                    //        Id = nextStep.CaseStatusId,
                    //        Name = nextStep.CaseStatus!.NameEN
                    //    }
                    //    : null,

                    ReliefSought = new SummaryView
                    {
                        Id = x.Case.ReliefSought!.Id,
                        Name = x.Case.ReliefSought.NameEN
                    },
                    ExpertWitness=x.Case.ExpertWitnessNameEn,
                    //ExpertWitness = x.Case.ExpertWitnessId.HasValue
                    //    ? new SummaryView
                    //    {
                    //        Id = x.Case.ExpertWitnessId.Value,
                    //        Name = x.Case.ExpertWitnessNameEn ?? string.Empty
                    //    }
                    //    : null,

                    IsCompleted = x.Case.IsCompleted,

                    CreatedOn = x.Case.CreatedOn,
                    CreatedById = x.Case.CreatedById,
                    CreatedByName = x.Case.CreatedByName,

                    UpdatedOn = x.Case.UpdatedOn,
                    UpdatedById = x.Case.UpdatedById,
                    UpdatedByName = x.Case.UpdatedByName,
                    CaseName=x.Case.CaseName,
                    ProjectCode=x.Case.ProjectCode,
                    ProjectName =x.Case.ProjectName,
                    UnitNumber =x.Case.UnitNumber,
                    UnitType =x.Case.UnitType,
                    LeadStatus =x.Case.LeadStatus,
                    BuyerName =x.Case.BuyerName,
                    BuyerNumber =x.Case.BuyerNumber,
                    JointBuyerName =x.Case.JointBuyerName,
                    JointBuyerMobile =x.Case.JointBuyerMobile,
                    SoldPrice =x.Case.SoldPrice,
                    LeadID =x.Case.LeadID,
                    ClosedDate = x.Case.ClosedDate,
                    ClosedStatus = x.Case.ClosedStatus,
                    IsClaimant=x.Case.IsClaimant,
                    MainCase = x.Case.MainCase == null ? null : new SummaryView
                                 {
                                 Id = (int)x.Case.MainCase.Id,
                                 Name = x.Case.MainCase.CaseName
                                 },
                };
                    }).ToList();

       
           
            //var entityDocument = new TblAuditLog
            //{
            //    UserId = currentUser.Email,
            //    Type = "GetAll",
            //    TableName = "cases",
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
        [RequiredPermission("cases.details")]
        [ProducesResponseType(typeof(Response<CaseDetailsQuery>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<CaseDetailsQuery?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.CaseRepository.GetAllQuerable()
                .Where(x => x.Id == id)
                .Include(x => x.Type)
                .Include(x => x.Level)
                .Include(x => x.Court)
                .Include(x => x.Status)
                .Include(x => x.ReliefSought)
                .Include(x => x.Notes)
                .Include(e => e.Team)
                .Include(e => e.MainCase)
                .Where(e => currentUser.IsSuperAdmin || e.Team.Select(t => t.UserId).Contains(currentUser.UserId))
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (entity == null)
                return StatusCode(result.Status, result);

            // Get workflow steps for current case type
            var workflowSteps = await unitOfWork.WorkflowRepository
                .GetAllQuerable()
                .Include(x => x.CaseStatus)
                .Where(x => x.CaseTypeId == entity.TypeId)
                .OrderBy(x => x.Order)
                .ToListAsync();

            // Get current workflow step
            var currentStep = workflowSteps
                .FirstOrDefault(x => x.CaseStatusId == entity.StatusId);

            // Get next workflow step
            var nextStep = currentStep == null
                ? null
                : workflowSteps.FirstOrDefault(x => x.Order == currentStep.Order + 1);

            var data = new CaseDetailsQuery
            {
                Id = entity.Id,
                Claimant = entity.Claimant,
                Defendant = entity.Defendant,
                ClaimValue = entity.ClaimValue,
                Summary = entity.Summary,
                UnitCode = entity.UnitCode,

                Type = new SummaryView
                {
                    Id = entity.Type!.Id,
                    Name = entity.Type.NameEN
                },

                Level = new SummaryView
                {
                    Id = entity.Level!.Id,
                    Name = entity.Level.NameEN
                },

                Court = new SummaryView
                {
                    Id = entity.Court!.Id,
                    Name = entity.Court.NameEN
                },

                Status = new SummaryView
                {
                    Id = entity.Status!.Id,
                    Name = entity.Status.NameEN
                },

                NextStatus = nextStep != null
                    ? new SummaryView
                    {
                        Id = nextStep.CaseStatusId,
                        Name = nextStep.CaseStatus!.NameEN
                    }
                    : null,

                ReliefSought = new SummaryView
                {
                    Id = entity.ReliefSought!.Id,
                    Name = entity.ReliefSought.NameEN
                },
                ExpertWitness= entity.ExpertWitnessNameEn,

                //ExpertWitness = entity.ExpertWitnessId.HasValue
                //    ? new SummaryView
                //    {
                //        Id = entity.ExpertWitnessId.Value,
                //        Name = entity.ExpertWitnessNameEn ?? string.Empty
                //    }
                //    : null,

                Notes = entity.Notes.Select(e => new CaseNoteView
                {
                    Id = e.Id,
                    Note = e.Note,
                    CreatedById = e.CreatedById,
                    CreatedByName = e.CreatedByName,
                    CreatedOn = e.CreatedOn
                }).ToList(),

                IsCompleted = entity.IsCompleted,

                CreatedOn = entity.CreatedOn,
                CreatedById = entity.CreatedById,
                CreatedByName = entity.CreatedByName,

                UpdatedOn = entity.UpdatedOn,
                UpdatedByName = entity.UpdatedByName,
                UpdatedById = entity.UpdatedById,
                CaseName = entity.CaseName,
                ProjectCode = entity.ProjectCode,
                ProjectName = entity.ProjectName,
                UnitNumber = entity.UnitNumber,
                UnitType = entity.UnitType,
                LeadStatus = entity.LeadStatus,
                BuyerName = entity.BuyerName,
                BuyerNumber = entity.BuyerNumber,
                JointBuyerName = entity.JointBuyerName,
                JointBuyerMobile = entity.JointBuyerMobile,
                SoldPrice = entity.SoldPrice,
                LeadID = entity.LeadID,
                IsClaimant = entity.IsClaimant,
                MainCase = entity.MainCase == null ? null : new SummaryView
                {
                    Id = (int)entity.MainCase.Id,
                    Name = entity.MainCase.CaseName
                },
            };

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;


            //var obj = new
            //{
            //    Id = entity.Id
            //};

            //string json = JsonSerializer.Serialize(obj);
            //var entityDocument = new TblAuditLog
            //{
            //    UserId = currentUser.Email,
            //    Type = "GetById",
            //    TableName = "cases",
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

        // ========================= GET BY ID =========================
        [HttpGet("{id}/audit-logs")]

        [RequiredPermission("case.auditlogs")]
        [ProducesResponseType(typeof(Response<IEnumerable<CaseAuditLogQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuditLogByIdAsync(long id)
        {
            var result = new Response<IEnumerable<CaseAuditLogQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var data = await unitOfWork.CaseAuditLogRepository.GetAllQuerable()
                .Where(e => e.CaseId == id)
                .Select(x => new CaseAuditLogQuery
                {
                    Id = x.Id,
                    Comment = x.Comment,
                    CreatedOn = x.CreatedOn,
                    CreatedById = x.CreatedById,
                    CreatedByName = x.CreatedByName
                })
                .ToListAsync();

            if (data == null)
                return StatusCode(result.Status, result);

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;


            return StatusCode(result.Status, result);
        }

        // ========================= CREATE =========================
        [HttpPost("create")]
        [RequiredPermission("cases.create")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAsync([FromBody] CaseCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };
            var LegalAdvisorUser = await unitOfWork.UserRepository.GetAllQuerable().AsNoTracking().FirstOrDefaultAsync(e => e.TypeId == 1);
            if (LegalAdvisorUser is null)
            {
                result.Status = (int)ResponseEnum.Failed;
                result.Title = "You need to create a legal manager before create any case";
                return StatusCode(result.Status, result);
            }


            // Get first workflow status
            //var firstWorkflowStep = await unitOfWork.WorkflowRepository
            //    .GetAllQuerable()
            //    .Where(x => x.CaseTypeId == model.TypeId)
            //    .OrderBy(x => x.Order)
            //    .FirstOrDefaultAsync();

            //if (firstWorkflowStep == null)
            //{
            //    result.Status = (int)ResponseEnum.BadRequest;
            //    result.Title = "No workflow configured for this case type";

            //    return StatusCode(result.Status, result);
            //}

            var createdOn = DateTime.Now;

            var entity = new TblCase
            {
                Claimant = model.Claimant,
                Defendant = model.Defendant,
                StatusId = model.StatusId,
                TypeId = model.TypeId,
                LevelId = model.LevelId,
                CourtId = model.CourtId,
                ClaimValue = model.ClaimValue,
                Summary = model.Summary,
                ReliefSoughtId = model.ReliefSoughtId,
                ExpertWitnessId = model.ExpertWitnessId,
                ExpertWitnessNameEn = model.ExpertWitnessNameEn,
                ExpertWitnessNameAr = model.ExpertWitnessNameAr,
                UnitCode = model.UnitCode,
                LeadID=model.LeadID,
                CaseName =model.CaseName,
                ProjectCode =model.ProjectCode,
                ProjectName =model.ProjectName,
                UnitNumber =model.UnitNumber,
                UnitType =model.UnitType,
                LeadStatus =model.LeadStatus,
                BuyerName =model.BuyerName,
                BuyerNumber =model.BuyerNumber,
                JointBuyerName =model.BuyerNumber,
                JointBuyerMobile =model.JointBuyerMobile,
                SoldPrice =model.SoldPrice,
                AuditLogs = new List<TblCaseAuditLog>
                {
                    new TblCaseAuditLog
                    {
                        Comment = "Case Created",
                        CreatedOn = createdOn,
                        CreatedById = currentUser.UserId,
                        CreatedByName = currentUser.UserName,
                    }
                },
                Team = new List<TblCaseTeam>()
                {
                    new TblCaseTeam
                    {
                        UserId = LegalAdvisorUser.Id,
                        CreatedOn = createdOn,
                        CreatedById = currentUser.UserId,
                        CreatedByName = currentUser.UserName,
                    }
                },

                CreatedOn = createdOn,
                CreatedById = currentUser.UserId,
                CreatedByName = currentUser.UserName,
                MainCaseId=model.MainCaseId,
                IsClaimant=model.IsClaimant
            };

            await unitOfWork.CaseRepository.AddAsync(entity);
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
                TableName = "case",
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
        [RequiredPermission("cases.update")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateCaseCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var refs = await referancesService.GetAllReferancesAsync();
            var entity = await unitOfWork.CaseRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            var obj = new
            {
                Id = entity.Id
            };

            string json = JsonSerializer.Serialize(obj);
            if (entity == null)
                return StatusCode(result.Status, result);

            var updatedOn = DateTime.Now;
            var auditLogEntities = new List<TblCaseAuditLog>();
            //var entityDocument = new TblAuditLog();
            var auditLogs = new List<TblAuditLog>();
            void AddAuditLog(string fieldName, object? oldValue, object? newValue)
            {
                auditLogEntities.Add(new TblCaseAuditLog
                {
                    CaseId = entity.Id,
                    Comment = $"{fieldName} updated from {oldValue ?? "NULL"} to {newValue ?? "NULL"}",
                    CreatedOn = updatedOn,
                    CreatedById = currentUser.UserId,
                    CreatedByName = currentUser.UserName,
                });


              
                

               
            }

            if (!string.IsNullOrWhiteSpace(model.Claimant) &&
                entity.Claimant != model.Claimant)
            {
                AddAuditLog("Claimant", entity.Claimant, model.Claimant);
                entity.Claimant = model.Claimant;
                auditLogs.Add( new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "case",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.Claimant,
                    NewValues = model.Claimant,
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }

            if (!string.IsNullOrWhiteSpace(model.Defendant) &&
                entity.Defendant != model.Defendant)
            {
                AddAuditLog("Defendant", entity.Defendant, model.Defendant);
                entity.Defendant = model.Defendant;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "case",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.Defendant,
                    NewValues = model.Defendant,
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }

            if (model.TypeId.HasValue &&
                entity.TypeId != model.TypeId.Value)
            {
                AddAuditLog("Type", refs.CaseTypes.FirstOrDefault(r => r.Id == entity.TypeId)?.NameEN, refs.CaseTypes.FirstOrDefault(r => r.Id == model.TypeId.Value)?.NameEN);
                entity.TypeId = model.TypeId.Value;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "case",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.TypeId.ToString(),
                    NewValues = model.TypeId.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }

            if (model.LevelId.HasValue &&
                entity.LevelId != model.LevelId.Value)
            {
                AddAuditLog("Level", refs.CaseLevels.FirstOrDefault(r => r.Id == entity.LevelId)?.NameEN, refs.CaseLevels.FirstOrDefault(r => r.Id == model.LevelId.Value)?.NameEN);
                entity.LevelId = model.LevelId.Value;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "case",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.LevelId.ToString(),
                    NewValues = model.LevelId.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

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
                    Type = "update",
                    TableName = "case",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.CourtId.ToString(),
                    NewValues = model.CourtId.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }

            if (model.ClaimValue.HasValue &&
                entity.ClaimValue != model.ClaimValue.Value)
            {
                AddAuditLog("ClaimValue", entity.ClaimValue, model.ClaimValue.Value);
                entity.ClaimValue = model.ClaimValue.Value;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "case",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.ClaimValue.ToString(),
                    NewValues = model.ClaimValue.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }

            if (!string.IsNullOrWhiteSpace(model.Summary) &&
                entity.Summary != model.Summary)
            {
                AddAuditLog("Summary", entity.Summary, model.Summary);
                entity.Summary = model.Summary;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "case",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.Summary.ToString(),
                    NewValues = model.Summary.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }

            if (model.ReliefSoughtId.HasValue &&
                entity.ReliefSoughtId != model.ReliefSoughtId.Value)
            {
                AddAuditLog("ReliefSought", refs.ReliefSoughts.FirstOrDefault(r => r.Id == entity.ReliefSoughtId)?.NameEN, refs.ReliefSoughts.FirstOrDefault(r => r.Id == model.ReliefSoughtId.Value)?.NameEN);
                entity.ReliefSoughtId = model.ReliefSoughtId.Value;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "case",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.ReliefSoughtId.ToString(),
                    NewValues = model.ReliefSoughtId.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }

            if (model.ExpertWitnessId.HasValue &&
                entity.ExpertWitnessId != model.ExpertWitnessId.Value)
            {
                AddAuditLog("ExpertWitness", entity.ExpertWitnessId, model.ExpertWitnessId.Value);
                entity.ExpertWitnessId = model.ExpertWitnessId.Value;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "case",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.ExpertWitnessId.ToString(),
                    NewValues = model.ExpertWitnessId.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }

            if (!string.IsNullOrWhiteSpace(model.ExpertWitnessNameEn) &&
                entity.ExpertWitnessNameEn != model.ExpertWitnessNameEn)
            {
                AddAuditLog("ExpertWitnessNameEn", entity.ExpertWitnessNameEn, model.ExpertWitnessNameEn);
                entity.ExpertWitnessNameEn = model.ExpertWitnessNameEn;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "case",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.ExpertWitnessNameEn.ToString(),
                    NewValues = model.ExpertWitnessNameEn.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }

            if (!string.IsNullOrWhiteSpace(model.ExpertWitnessNameAr) &&
                entity.ExpertWitnessNameAr != model.ExpertWitnessNameAr)
            {
                AddAuditLog("ExpertWitnessNameAr", entity.ExpertWitnessNameAr, model.ExpertWitnessNameAr);
                entity.ExpertWitnessNameAr = model.ExpertWitnessNameAr;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "case",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.ExpertWitnessNameAr.ToString(),
                    NewValues = model.ExpertWitnessNameAr.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }

            if (!string.IsNullOrWhiteSpace(model.UnitCode) &&
                entity.UnitCode != model.UnitCode)
            {
                AddAuditLog("UnitCode", entity.UnitCode, model.UnitCode);
                entity.UnitCode = model.UnitCode;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "case",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.UnitCode.ToString(),
                    NewValues = model.UnitCode.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }
            
            if (!string.IsNullOrWhiteSpace(model.CaseName) &&
                entity.CaseName != model.CaseName)
            {
                AddAuditLog("CaseName", entity.CaseName, model.CaseName);
                entity.CaseName = model.CaseName;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "case",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.CaseName.ToString(),
                    NewValues = model.CaseName.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
               
            }
            if (model.MainCaseId.HasValue &&
                entity.MainCaseId != model.MainCaseId.Value)
            {
                AddAuditLog("maincase", entity.MainCaseId, model.MainCaseId.Value);
                entity.MainCaseId = model.MainCaseId.Value;
                auditLogs.Add(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "case",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.MainCaseId.ToString(),
                    NewValues = model.MainCaseId.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = json,
                    IsArchived = false,

                });
            }



            entity.IsClaimant = model.IsClaimant;
            entity.UpdatedOn = updatedOn;
            entity.UpdatedById = currentUser.UserId;
            entity.UpdatedByName = currentUser.UserName;

            unitOfWork.CaseRepository.Update(entity);

            if (auditLogEntities.Any())
                await unitOfWork.CaseAuditLogRepository.AddRangeAsync(auditLogEntities);
            if (auditLogs.Any())
                await unitOfWork.AuditLogRepository.AddRangeAsync(auditLogs);
            await unitOfWork.CompleteAsync();

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Updated";
            result.Data = true;

            return StatusCode(result.Status, result);
        }


        // ========================= Close Case =========================
        [HttpPost("{id}/close")]
        [RequiredPermission("cases.update")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CloseCaseAsync(long id, [FromBody] CloseCaseCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var refs = await referancesService.GetAllReferancesAsync();
            var entity = await unitOfWork.CaseRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            var updatedOn = DateTime.Now;
            var auditLogEntities = new List<TblCaseAuditLog>();

            void AddAuditLog(string fieldName, object? oldValue, object? newValue)
            {
                auditLogEntities.Add(new TblCaseAuditLog
                {
                    CaseId = entity.Id,
                    Comment = $"{fieldName} updated from {oldValue ?? "NULL"} to {newValue ?? "NULL"}",
                    CreatedOn = updatedOn,
                    CreatedById = currentUser.UserId,
                    CreatedByName = currentUser.UserName,
                });
            }

          
                AddAuditLog("IsCompeleted", entity.IsCompleted, model.IsCompleted);
                entity.IsCompleted = model.IsCompleted;

            if (model.IsCompleted == false)
            {
                entity.ClosedStatus = null;
                entity.ClosedDate = null;
            }
            else
            {
                entity.ClosedStatus = (CASEENUM)model.ClosedStatus;
                entity.ClosedDate = model.ClosedDate;
            }
            entity.UpdatedOn = updatedOn;
            entity.UpdatedById = currentUser.UserId;
            entity.UpdatedByName = currentUser.UserName;

            unitOfWork.CaseRepository.Update(entity);

            if (auditLogEntities.Any())
                await unitOfWork.CaseAuditLogRepository.AddRangeAsync(auditLogEntities);

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
                Type = "close",
                TableName = "Case",
                ActionType = "close",
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
        [HttpPost("{id}/update-status")]
        [RequiredPermission("cases.update.status")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateStatusAsync(long id)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var refs = await referancesService.GetAllReferancesAsync();

            var entity = await unitOfWork.CaseRepository
                .GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            if (entity.IsCompleted)
            {
                result.Status = (int)ResponseEnum.BadRequest;
                result.Title = "Case is already completed";

                return StatusCode(result.Status, result);
            }

            // Get workflow steps
            var workflowSteps = await unitOfWork.WorkflowRepository
                .GetAllQuerable()
                .Where(x => x.CaseTypeId == entity.TypeId)
                .OrderBy(x => x.Order)
                .ToListAsync();

            if (!workflowSteps.Any())
            {
                result.Status = (int)ResponseEnum.BadRequest;
                result.Title = "No workflow configured for this case type";

                return StatusCode(result.Status, result);
            }

            // Current step
            var currentStep = workflowSteps
                .FirstOrDefault(x => x.CaseStatusId == entity.StatusId);

            if (currentStep == null)
            {
                result.Status = (int)ResponseEnum.BadRequest;
                result.Title = "Current case status is not part of workflow";

                return StatusCode(result.Status, result);
            }

            // Next step
            var nextStep = workflowSteps
                .FirstOrDefault(x => x.Order == currentStep.Order + 1);

            if (nextStep == null)
            {
                result.Status = (int)ResponseEnum.BadRequest;
                result.Title = "No next status available";

                return StatusCode(result.Status, result);
            }

            var updatedOn = DateTime.Now;

            var oldStatusId = entity.StatusId;
            entity.StatusId = nextStep.CaseStatusId;

            // If moved to final workflow step => complete case
            var isFinalStep = !workflowSteps
                .Any(x => x.Order > nextStep.Order);

            if (isFinalStep)
            {
                entity.IsCompleted = true;
            }

            var oldStatusName = refs.CaseStatuses
                .FirstOrDefault(r => r.Id == oldStatusId)?.NameEN;

            var newStatusName = refs.CaseStatuses
                .FirstOrDefault(r => r.Id == nextStep.CaseStatusId)?.NameEN;

            await unitOfWork.CaseAuditLogRepository.AddAsync(new TblCaseAuditLog
            {
                CaseId = entity.Id,
                Comment = $"Status updated from {oldStatusName ?? "NULL"} to {newStatusName ?? "NULL"}",
                CreatedOn = updatedOn,
                CreatedById = currentUser.UserId,
                CreatedByName = currentUser.UserName,
            });

            if (isFinalStep)
            {
                await unitOfWork.CaseAuditLogRepository.AddAsync(new TblCaseAuditLog
                {
                    CaseId = entity.Id,
                    Comment = "Case completed",
                    CreatedOn = updatedOn,
                    CreatedById = currentUser.UserId,
                    CreatedByName = currentUser.UserName,
                });
            }

            entity.UpdatedOn = updatedOn;
            entity.UpdatedById = currentUser.UserId;
            entity.UpdatedByName = currentUser.UserName;

            unitOfWork.CaseRepository.Update(entity);

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
                Type = "UpdateStatus",
                TableName = "Case",
                ActionType = "UpdateStatus",
                DateTime = DateTime.Now,
                OldValues = oldStatusName,
                NewValues = newStatusName,
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
        [RequiredPermission("cases.delete")]
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

            var entity = await unitOfWork.CaseRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            await unitOfWork.CaseAuditLogRepository.AddAsync(new TblCaseAuditLog
            {
                CaseId = entity.Id,
                Comment = $"Case deleted. Claimant: {entity.Claimant}, Defendant: {entity.Defendant}",
                CreatedOn = DateTime.Now,
                CreatedById = currentUser.UserId,
                CreatedByName = currentUser.UserName,
            });

            unitOfWork.CaseRepository.Delete(entity);

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
                Type = "delete",
                TableName = "Case",
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

        //[HttpGet("GetCasesDashBoard")]
        ////[RequiredPermission("cases.get")]
        //[ProducesResponseType(typeof(QueryResult<CaseQuery>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> GetCasesDashBoardAsync()
        //{
        //    var currentUser = await httpContext.GetCurrentUser();

        //    var result = new QueryResult<CaseQuery>()
        //    {
        //        Status = (int)ResponseEnum.Succeeded,
        //        Title = "Data Retrieved"
        //    };

        //    var rows = unitOfWork.CaseRepository.GetAllQuerable()
        //        .Include(e => e.Type)
        //        .Include(e => e.Level)
        //        .Include(e => e.Court)
        //        .Include(e => e.Status)
        //        .Include(e => e.ReliefSought)
        //        .Include(e => e.Team)
        //        .Where(e => currentUser.IsSuperAdmin || e.Team.Select(t => t.UserId).Contains(currentUser.UserId))
        //        .AsNoTracking();


        //    var totalItems = await rows.CountAsync();

        //    if (totalItems <= 0)
        //    {
        //        result.Title = "No Data Found";
        //        return StatusCode(result.Status, result);
        //    }


        //    // Load workflow steps
        //    var workflows = await unitOfWork.WorkflowRepository
        //        .GetAllQuerable()
        //        .Include(x => x.CaseStatus)
        //        .ToListAsync();

        //    // Projection
        //    var data = await rows.Select(x => new
        //    {
        //        Case = x,
        //    }).ToListAsync();

        //    result.Data.Rows = data.Select(x =>
        //    {
        //        var currentWorkflow = workflows
        //            .Where(w => w.CaseTypeId == x.Case.TypeId)
        //            .OrderBy(w => w.Order)
        //            .ToList();

        //        var currentStep = currentWorkflow
        //            .FirstOrDefault(w => w.CaseStatusId == x.Case.StatusId);

        //        var nextStep = currentStep == null
        //            ? null
        //            : currentWorkflow.FirstOrDefault(w => w.Order == currentStep.Order + 1);

        //        return new CaseQuery
        //        {
        //            Id = x.Case.Id,
        //            Claimant = x.Case.Claimant,
        //            Defendant = x.Case.Defendant,
        //            ClaimValue = x.Case.ClaimValue,
        //            Summary = x.Case.Summary,
        //            UnitCode = x.Case.UnitCode,

        //            Type = new SummaryView
        //            {
        //                Id = x.Case.Type!.Id,
        //                Name = x.Case.Type.NameEN
        //            },

        //            Level = new SummaryView
        //            {
        //                Id = x.Case.Level!.Id,
        //                Name = x.Case.Level.NameEN
        //            },

        //            Court = new SummaryView
        //            {
        //                Id = x.Case.Court!.Id,
        //                Name = x.Case.Court.NameEN
        //            },

        //            Status = new SummaryView
        //            {
        //                Id = x.Case.Status!.Id,
        //                Name = x.Case.Status.NameEN
        //            },

        //            NextStatus = nextStep != null
        //                ? new SummaryView
        //                {
        //                    Id = nextStep.CaseStatusId,
        //                    Name = nextStep.CaseStatus!.NameEN
        //                }
        //                : null,

        //            ReliefSought = new SummaryView
        //            {
        //                Id = x.Case.ReliefSought!.Id,
        //                Name = x.Case.ReliefSought.NameEN
        //            },

        //            ExpertWitness = x.Case.ExpertWitnessId.HasValue
        //                ? new SummaryView
        //                {
        //                    Id = x.Case.ExpertWitnessId.Value,
        //                    Name = x.Case.ExpertWitnessNameEn ?? string.Empty
        //                }
        //                : null,

        //            IsCompleted = x.Case.IsCompleted,
        //            ProjectName=x.Case.ProjectName,

        //            CreatedOn = x.Case.CreatedOn,
        //            CreatedById = x.Case.CreatedById,
        //            CreatedByName = x.Case.CreatedByName,

        //            UpdatedOn = x.Case.UpdatedOn,
        //            UpdatedById = x.Case.UpdatedById,
        //            UpdatedByName = x.Case.UpdatedByName,
        //        };
        //    }).ToList();



        //    return StatusCode(result.Status, result);
        //}


        [HttpGet("ExportCasesDashBoard")]
        public async Task<IActionResult> ExportCasesDashBoardAsync([FromQuery] CaseFilter filter)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var rows = unitOfWork.CaseRepository.GetAllQuerable()
                .Include(e => e.Type)
                .Include(e => e.Level)
                .Include(e => e.Court)
                .Include(e => e.Status)
                .Include(e => e.ReliefSought)
                .Include(e => e.Team)
                .Where(e => currentUser.IsSuperAdmin ||
                          e.CreatedById == currentUser.UserId)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                rows = rows.Where(x =>
                    (x.Claimant + x.Defendant + (x.UnitCode ?? "")).Contains(filter.SearchText));
            }

            if (filter.TypeId.HasValue)
                rows = rows.Where(x => x.TypeId == filter.TypeId.Value);

            if (filter.LevelId.HasValue)
                rows = rows.Where(x => x.LevelId == filter.LevelId.Value);

            if (filter.CourtId.HasValue)
                rows = rows.Where(x => x.CourtId == filter.CourtId.Value);

            if (filter.StatusId.HasValue)
                rows = rows.Where(x => x.StatusId == filter.StatusId.Value);

            if (filter.ReliefSoughtId.HasValue)
                rows = rows.Where(x => x.ReliefSoughtId == filter.ReliefSoughtId.Value);
            if (!string.IsNullOrEmpty(filter.LeadID))
                rows = rows.Where(x => x.LeadID.Contains(filter.LeadID));
            if (filter.MainCaseId.HasValue)
                rows = rows.Where(x => x.MainCaseId == filter.MainCaseId.Value);

            var workflows = await unitOfWork.WorkflowRepository
                .GetAllQuerable()
                .Include(x => x.CaseStatus)
                .ToListAsync();

            var data = await rows.ToListAsync();

            var templatePath = Path.Combine(
                         Directory.GetCurrentDirectory(),
                                "Templates",
                             "CasesDashboard.xlsx");

            string outputPath = Path.Combine(
                Path.GetTempPath(),
                $"CasesDashboard_{Guid.NewGuid()}.xlsx");

            System.IO.File.Copy(templatePath, outputPath, true);

            using (var workbook = new XLWorkbook(outputPath))
            {
                var worksheet = workbook.Worksheet(1);

                worksheet.Cell(2, 6).Value = DateTime.Now;
                worksheet.Cell(2, 6).Style.DateFormat.Format = "dd-MM-yyyy HH:mm";

                int row = 5;

                foreach (var c in data)
                {
                    var currentWorkflow = workflows
                        .Where(w => w.CaseTypeId == c.TypeId)
                        .OrderBy(w => w.Order)
                        .ToList();

                    var currentStep = currentWorkflow
                        .FirstOrDefault(w => w.CaseStatusId == c.StatusId);

                    var nextStep = currentStep == null
                        ? null
                        : currentWorkflow.FirstOrDefault(w => w.Order == currentStep.Order + 1);

                    worksheet.Cell(row, 1).Value = c.Id;
                    worksheet.Cell(row, 2).Value = c.CaseName;
                    worksheet.Cell(row, 3).Value = c.Claimant;
                    worksheet.Cell(row, 4).Value = c.Defendant;
                    worksheet.Cell(row, 5).Value = c.Type?.NameEN;
                    worksheet.Cell(row, 6).Value = c.Level?.NameEN;
                    worksheet.Cell(row, 7).Value = c.Court?.NameEN;
                    worksheet.Cell(row, 8).Value = c.Status?.NameEN;
                    //worksheet.Cell(row, 9).Value = nextStep?.CaseStatus?.NameEN;
                    worksheet.Cell(row, 10).Value = c.ReliefSought?.NameEN;
                    worksheet.Cell(row, 11).Value = c.ClaimValue;
                    worksheet.Cell(row, 12).Value = c.ProjectName;
                    worksheet.Cell(row, 13).Value = c.UnitCode;
                    worksheet.Cell(row, 14).Value = c.CreatedByName;
                    worksheet.Cell(row, 15).Value = c.CreatedOn;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                workbook.Save();
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(outputPath);

            return PhysicalFile(outputPath,"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                $"CasesDashboard_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
           
        }


        [HttpGet("SubCases")]
        [RequiredPermission("cases.get")]
        [ProducesResponseType(typeof(QueryResult<CaseQuery>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SubCasesAsync([FromQuery] SubCaseFilter filter)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new QueryResult<CaseQuery>()
            {
                Status = (int)ResponseEnum.Succeeded,
                Title = "Data Retrieved"
            };

            var rows = unitOfWork.CaseRepository.GetAllQuerable()
                .Include(e => e.Type)
                .Include(e => e.Level)
                .Include(e => e.Court)
                .Include(e => e.Status)
                .Include(e => e.ReliefSought)
                .Include(e => e.Team)
                .Where(e => (currentUser.IsSuperAdmin || e.CreatedById==currentUser.UserId) )
                .AsNoTracking();

            // Filtering
          

            if (filter.MainCaseId.HasValue)
                rows = rows.Where(x => x.MainCaseId == filter.MainCaseId.Value);


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
                "Claimant" => filter.IsAscending ? rows.OrderBy(x => x.Claimant) : rows.OrderByDescending(x => x.Claimant),
                "Defendant" => filter.IsAscending ? rows.OrderBy(x => x.Defendant) : rows.OrderByDescending(x => x.Defendant),
                "CreatedOn" => filter.IsAscending ? rows.OrderBy(x => x.CreatedOn) : rows.OrderByDescending(x => x.CreatedOn),
                _ => filter.IsAscending ? rows.OrderBy(x => x.Id) : rows.OrderByDescending(x => x.Id),
            };

            // Paging
            result.Data.Count = totalItems;

            if (filter.Size > 0)
            {
                rows = rows.Skip(filter.Index * filter.Size).Take(filter.Size);
            }

            // Load workflow steps
            //var workflows = await unitOfWork.WorkflowRepository
            //    .GetAllQuerable()
            //    .Include(x => x.CaseStatus)
            //    .ToListAsync();

            // Projection
            var data = await rows.Select(x => new
            {
                Case = x,
            }).ToListAsync();

            result.Data.Rows = data.Select(x =>
            {
                //var currentWorkflow = workflows
                //    .Where(w => w.CaseTypeId == x.Case.TypeId)
                //    .OrderBy(w => w.Order)
                //    .ToList();

                //var currentStep = currentWorkflow
                //    .FirstOrDefault(w => w.CaseStatusId == x.Case.StatusId);

                //var nextStep = currentStep == null
                //    ? null
                //    : currentWorkflow.FirstOrDefault(w => w.Order == currentStep.Order + 1);

                return new CaseQuery
                {
                    Id = x.Case.Id,
                    Claimant = x.Case.Claimant,
                    Defendant = x.Case.Defendant,
                    ClaimValue = x.Case.ClaimValue,
                    Summary = x.Case.Summary,
                    UnitCode = x.Case.UnitCode,

                    Type = new SummaryView
                    {
                        Id = x.Case.Type!.Id,
                        Name = x.Case.Type.NameEN
                    },

                    Level = new SummaryView
                    {
                        Id = x.Case.Level!.Id,
                        Name = x.Case.Level.NameEN
                    },

                    Court = new SummaryView
                    {
                        Id = x.Case.Court!.Id,
                        Name = x.Case.Court.NameEN
                    },

                    Status = new SummaryView
                    {
                        Id = x.Case.Status!.Id,
                        Name = x.Case.Status.NameEN
                    },

                    //NextStatus = nextStep != null
                    //    ? new SummaryView
                    //    {
                    //        Id = nextStep.CaseStatusId,
                    //        Name = nextStep.CaseStatus!.NameEN
                    //    }
                    //    : null,

                    ReliefSought = new SummaryView
                    {
                        Id = x.Case.ReliefSought!.Id,
                        Name = x.Case.ReliefSought.NameEN
                    },
                    ExpertWitness = x.Case.ExpertWitnessNameEn,
                    //ExpertWitness = x.Case.ExpertWitnessId.HasValue
                    //    ? new SummaryView
                    //    {
                    //        Id = x.Case.ExpertWitnessId.Value,
                    //        Name = x.Case.ExpertWitnessNameEn ?? string.Empty
                    //    }
                    //    : null,

                    IsCompleted = x.Case.IsCompleted,

                    CreatedOn = x.Case.CreatedOn,
                    CreatedById = x.Case.CreatedById,
                    CreatedByName = x.Case.CreatedByName,

                    UpdatedOn = x.Case.UpdatedOn,
                    UpdatedById = x.Case.UpdatedById,
                    UpdatedByName = x.Case.UpdatedByName,
                    CaseName = x.Case.CaseName,
                    ProjectCode = x.Case.ProjectCode,
                    ProjectName = x.Case.ProjectName,
                    UnitNumber = x.Case.UnitNumber,
                    UnitType = x.Case.UnitType,
                    LeadStatus = x.Case.LeadStatus,
                    BuyerName = x.Case.BuyerName,
                    BuyerNumber = x.Case.BuyerNumber,
                    JointBuyerName = x.Case.JointBuyerName,
                    JointBuyerMobile = x.Case.JointBuyerMobile,
                    SoldPrice = x.Case.SoldPrice,
                    LeadID = x.Case.LeadID,
                    ClosedDate = x.Case.ClosedDate,
                    ClosedStatus = x.Case.ClosedStatus,
                    IsClaimant = x.Case.IsClaimant
                };
            }).ToList();



            //var entityDocument = new TblAuditLog
            //{
            //    UserId = currentUser.Email,
            //    Type = "GetAll",
            //    TableName = "cases",
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


        [HttpGet("GetList")]
        [RequiredPermission("cases.get")]
        [ProducesResponseType(typeof(QueryResult<CaseQuery>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListAsync([FromQuery] CaseFilter filter)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new QueryResult<CaseList>()
            {
                Status = (int)ResponseEnum.Succeeded,
                Title = "Data Retrieved"
            };

            var rows = unitOfWork.CaseRepository.GetAllQuerable()
                .Include(e => e.Type)
                .Include(e => e.Level)
                .Include(e => e.Court)
                .Include(e => e.Status)
                .Include(e => e.ReliefSought)
                .Include(e => e.Team)
                .Where(e => currentUser.IsSuperAdmin || e.Team.Select(t => t.UserId).Contains(currentUser.UserId))
                .AsNoTracking();

           

         
            // Projection
            var data = await rows.Select(x => new
            {
                Case = x,
            }).ToListAsync();

            result.Data.Rows = data.Select(x =>
            {
               
                return new CaseList
                {
                    Id = x.Case.Id,
                    CaseName = x.Case.CaseName,
                };
            }).ToList();

           
            return StatusCode(result.Status, result);
        }

    }
}
