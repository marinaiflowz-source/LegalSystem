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
        [ProducesResponseType( typeof(QueryResult<CaseQuery>),StatusCodes.Status200OK)]
        [ProducesResponseType( typeof(ProblemDetails),StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync([FromQuery] CaseFilter filter)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new QueryResult<CaseQuery>
            {
                Status = (int)ResponseEnum.Succeeded,
                Title = "Data Retrieved"
            };

            var rows = unitOfWork.CaseRepository
                .GetAllQuerable()
                .Include(x => x.Type)
                .Include(x => x.Court)
                .Include(x => x.Status)
                .Include(x => x.ReliefSought)
                .Include(x => x.Team)
                .Include(x => x.MainCase)
                .Include(x => x.AssignedUser)
                .AsNoTracking();

            #region General Search

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var search = filter.SearchText.Trim();

                rows = rows.Where(x =>
                    (x.Claimant != null && x.Claimant.Contains(search)) ||

                    (x.Defendant != null && x.Defendant.Contains(search)) ||

                    (x.CaseName != null && x.CaseName.Contains(search)) ||

                    (x.Summary != null && x.Summary.Contains(search)) ||

                    (x.UnitCode != null && x.UnitCode.Contains(search)) ||

                    (x.ProjectCode != null && x.ProjectCode.Contains(search)) ||

                    (x.ProjectName != null && x.ProjectName.Contains(search)) ||

                    (x.UnitNumber != null && x.UnitNumber.Contains(search)) ||

                    (x.UnitType != null && x.UnitType.Contains(search)) ||

                    (x.LeadID != null && x.LeadID.Contains(search)) ||

                    (x.LeadStatus != null && x.LeadStatus.Contains(search)) ||

                    (x.BuyerName != null && x.BuyerName.Contains(search)) ||

                    (x.BuyerNumber != null && x.BuyerNumber.Contains(search)) ||

                    (x.JointBuyerName != null && x.JointBuyerName.Contains(search)) ||

                    (x.JointBuyerMobile != null && x.JointBuyerMobile.Contains(search)) ||

                    (x.ExpertWitnessNameEn != null && x.ExpertWitnessNameEn.Contains(search)) ||

                    (x.ExpertWitnessNameAr != null && x.ExpertWitnessNameAr.Contains(search)) ||
                    (x.CloseReason != null && x.CloseReason.Contains(search)) ||

                    (x.Note != null && x.Note.Contains(search)) ||

                    (x.CreatedByName != null && x.CreatedByName.Contains(search)) ||

                    (x.UpdatedByName != null && x.UpdatedByName.Contains(search)) ||

                    (x.Type != null && x.Type.NameEN.Contains(search)) ||
                    (x.Type != null && x.Type.NameAR.Contains(search)) ||

                    (x.Court != null && x.Court.NameEN.Contains(search)) ||
                    (x.Court != null && x.Court.NameAR.Contains(search)) ||

                    (x.Status != null && x.Status.NameEN.Contains(search)) ||
                    (x.Status != null && x.Status.NameAR.Contains(search)) ||

                    (x.ReliefSought != null && x.ReliefSought.NameEN.Contains(search)) ||
                    (x.ReliefSought != null && x.ReliefSought.NameAR.Contains(search)) ||

                    (x.MainCase != null && x.MainCase.CaseName.Contains(search)) ||

                    (x.AssignedUser != null && x.AssignedUser.NameEn.Contains(search)) ||
                    (x.AssignedUser != null && x.AssignedUser.NameAr.Contains(search))
                );
            }
           
            #endregion

            #region Lookup Filters

            if (filter.TypeId.HasValue)
            {
                rows = rows.Where(x =>
                    x.TypeId == filter.TypeId.Value);
            }

            if (filter.CourtId.HasValue)
            {
                rows = rows.Where(x =>
                    x.CourtId == filter.CourtId.Value);
            }

            if (filter.StatusId.HasValue)
            {
                rows = rows.Where(x =>
                    x.StatusId == filter.StatusId.Value);
            }

            if (filter.AssignedUserId.HasValue)
            {
                rows = rows.Where(x =>
                    x.AssignedUserId ==
                    filter.AssignedUserId.Value);
            }

            if (filter.ReliefSoughtId.HasValue)
            {
                rows = rows.Where(x =>
                    x.ReliefSoughtId ==
                    filter.ReliefSoughtId.Value);
            }

            if (filter.MainCaseId.HasValue)
            {
                rows = rows.Where(x =>
                    x.MainCaseId == filter.MainCaseId.Value);
            }

            #endregion

          

            #region Numeric Filters

           
            #endregion

            #region Boolean Filters

            if (filter.IsCompleted.HasValue)
            {
                rows = rows.Where(x =>
                    x.IsCompleted ==
                    filter.IsCompleted.Value);
            }

            if (filter.IsClaimant.HasValue)
            {
                rows = rows.Where(x =>
                    x.IsClaimant ==
                    filter.IsClaimant.Value);
            }

            #endregion

            #region Date Filters


            if (filter.CreatedFrom.HasValue)
            {
                var createdFrom = filter.CreatedFrom.Value.Date;
                rows = rows.Where(x => x.CreatedOn >= createdFrom);
            }

            if (filter.CreatedTo.HasValue)
            {
                var createdToExclusive = filter.CreatedTo.Value.Date.AddDays(1);
                rows = rows.Where(x => x.CreatedOn < createdToExclusive);
            }

            #endregion

            var totalItems = await rows.CountAsync();

            result.Data.Count = totalItems;

            if (totalItems == 0)
            {
                result.Title = "No Data Found";
                result.Data.Rows = new List<CaseQuery>();

                return StatusCode(result.Status, result);
            }

            #region Ordering

            if (string.IsNullOrWhiteSpace(filter.SortBy))
            {
                filter.SortBy = "Id";
                filter.IsAscending = false;
            }

            rows = filter.SortBy.Trim().ToLower() switch
            {
                "claimant" => filter.IsAscending
                    ? rows.OrderBy(x => x.Claimant)
                    : rows.OrderByDescending(x => x.Claimant),

                "defendant" => filter.IsAscending
                    ? rows.OrderBy(x => x.Defendant)
                    : rows.OrderByDescending(x => x.Defendant),

                "casename" => filter.IsAscending
                    ? rows.OrderBy(x => x.CaseName)
                    : rows.OrderByDescending(x => x.CaseName),

                "claimvalue" => filter.IsAscending
                    ? rows.OrderBy(x => x.ClaimValue)
                    : rows.OrderByDescending(x => x.ClaimValue),

                "soldprice" => filter.IsAscending
                    ? rows.OrderBy(x => x.SoldPrice)
                    : rows.OrderByDescending(x => x.SoldPrice),

                "projectcode" => filter.IsAscending
                    ? rows.OrderBy(x => x.ProjectCode)
                    : rows.OrderByDescending(x => x.ProjectCode),

                "projectname" => filter.IsAscending
                    ? rows.OrderBy(x => x.ProjectName)
                    : rows.OrderByDescending(x => x.ProjectName),

                "unitcode" => filter.IsAscending
                    ? rows.OrderBy(x => x.UnitCode)
                    : rows.OrderByDescending(x => x.UnitCode),

                "unitnumber" => filter.IsAscending
                    ? rows.OrderBy(x => x.UnitNumber)
                    : rows.OrderByDescending(x => x.UnitNumber),

                "leadid" => filter.IsAscending
                    ? rows.OrderBy(x => x.LeadID)
                    : rows.OrderByDescending(x => x.LeadID),

                "closeddate" => filter.IsAscending
                    ? rows.OrderBy(x => x.ClosedDate)
                    : rows.OrderByDescending(x => x.ClosedDate),

                "updatedon" => filter.IsAscending
                    ? rows.OrderBy(x => x.UpdatedOn)
                    : rows.OrderByDescending(x => x.UpdatedOn),

                "createdon" => filter.IsAscending
                    ? rows.OrderBy(x => x.CreatedOn)
                    : rows.OrderByDescending(x => x.CreatedOn),

                _ => filter.IsAscending
                    ? rows.OrderBy(x => x.Id)
                    : rows.OrderByDescending(x => x.Id)
            };

            #endregion

            #region Paging

            if (filter.Size > 0)
            {
                var pageIndex = filter.Index < 0
                    ? 0
                    : filter.Index;

                rows = rows
                    .Skip(pageIndex * filter.Size)
                    .Take(filter.Size);
            }

            #endregion

            var data = await rows
                .Select(x => new CaseQuery
                {
                    Id = x.Id,
                    Claimant = x.Claimant,
                    Defendant = x.Defendant,
                    ClaimValue = x.ClaimValue,
                    Summary = x.Summary,
                    UnitCode = x.UnitCode,

                    Type = x.Type == null
                        ? null
                        : new SummaryView
                        {
                            Id = x.Type.Id,
                            Name = x.Type.NameEN
                        },

                    Court = x.Court == null
                        ? null
                        : new SummaryView
                        {
                            Id = x.Court.Id,
                            Name = x.Court.NameEN
                        },

                    Status = x.Status == null
                        ? null
                        : new SummaryView
                        {
                            Id = x.Status.Id,
                            Name = x.Status.NameEN
                        },

                    ReliefSought = x.ReliefSought == null
                        ? null
                        : new SummaryView
                        {
                            Id = x.ReliefSought.Id,
                            Name = x.ReliefSought.NameEN
                        },

                    ExpertWitness = x.ExpertWitnessNameEn,

                    IsCompleted = x.IsCompleted,

                    CreatedOn = x.CreatedOn,
                    CreatedById = x.CreatedById,
                    CreatedByName = x.CreatedByName,

                    UpdatedOn = x.UpdatedOn,
                    UpdatedById = x.UpdatedById,
                    UpdatedByName = x.UpdatedByName,

                    CaseName = x.CaseName,
                    ProjectCode = x.ProjectCode,
                    ProjectName = x.ProjectName,
                    UnitNumber = x.UnitNumber,
                    UnitType = x.UnitType,

                    LeadStatus = x.LeadStatus,
                    BuyerName = x.BuyerName,
                    BuyerNumber = x.BuyerNumber,
                    JointBuyerName = x.JointBuyerName,
                    JointBuyerMobile = x.JointBuyerMobile,

                    SoldPrice = x.SoldPrice,
                    LeadID = x.LeadID,

                    ClosedDate = x.ClosedDate,
                    ClosedStatus = x.ClosedStatus,
                    IsClaimant = x.IsClaimant,

                    MainCase = x.MainCase == null
                        ? null
                        : new SummaryView
                        {
                            Id = (int)x.MainCase.Id,
                            Name = x.MainCase.CaseName
                        },

                    AssignedUser = x.AssignedUser == null
                        ? null
                        : new SummaryUserView
                        {
                            Id = x.AssignedUser.Id,
                            Name = x.AssignedUser.NameEn,
                            UserId = x.AssignedUser.RefId
                        },

                    Note = x.Note,
                    CloseReason = x.CloseReason
                })
                .ToListAsync();

            var caseIds = data
                .Select(x => x.Id)
                .ToList();

            var activeTaskCounts =
                await unitOfWork.CaseTaskRepository
                    .GetAllQuerable()
                    .AsNoTracking()
                    .Where(x =>
                        caseIds.Contains(x.CaseId) &&
                        !x.IsClosed)
                    .GroupBy(x => x.CaseId)
                    .Select(x => new
                    {
                        CaseId = x.Key,
                        TotalActiveTasks = x.Count()
                    })
                    .ToDictionaryAsync(
                        x => x.CaseId,
                        x => x.TotalActiveTasks);

            foreach (var item in data)
            {
                item.TotalActiveTasks =
                    activeTaskCounts.TryGetValue(
                        item.Id,
                        out var totalActiveTasks)
                        ? totalActiveTasks
                        : 0;
            }

            result.Data.Rows = data;

            return StatusCode(result.Status, result);
        }




        //[HttpGet]
        //[RequiredPermission("cases.get")]
        //[ProducesResponseType(typeof(QueryResult<CaseQuery>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> GetAsync([FromQuery] CaseFilter filter)
        //{
        //    var currentUser = await httpContext.GetCurrentUser();

        //    var result = new QueryResult<CaseQuery>()
        //    {
        //        Status = (int)ResponseEnum.Succeeded,
        //        Title = "Data Retrieved"
        //    };

        //    var rows = unitOfWork.CaseRepository.GetAllQuerable()
        //        .Include(e => e.Type)
        //        //.Include(e => e.Level)
        //        .Include(e => e.Court)
        //        .Include(e => e.Status)
        //        .Include(e => e.ReliefSought)
        //        .Include(e => e.Team)
        //        .Include(x => x.MainCase)
        //        .Include(e => e.AssignedUser)
        //        //.Where(e => currentUser.IsSuperAdmin)
        //        .AsNoTracking();

        //    // Filtering
        //    if (!string.IsNullOrWhiteSpace(filter.SearchText))
        //    {
        //        rows = rows.Where(x =>
        //            (x.Claimant+x.CaseName + x.Defendant + (x.UnitCode ?? "")).Contains(filter.SearchText));
        //    }

        //    if (filter.TypeId.HasValue)
        //        rows = rows.Where(x => x.TypeId == filter.TypeId.Value);


        //    //if (filter.LevelId.HasValue)
        //    //    rows = rows.Where(x => x.LevelId == filter.LevelId.Value);

        //    if (filter.CourtId.HasValue)
        //        rows = rows.Where(x => x.CourtId == filter.CourtId.Value);

        //    if (filter.StatusId.HasValue)
        //        rows = rows.Where(x => x.StatusId == filter.StatusId.Value);

        //    if (filter.AssignedUserId.HasValue)
        //        rows = rows.Where(x => x.AssignedUserId == filter.AssignedUserId.Value);

        //    if (filter.ReliefSoughtId.HasValue)
        //        rows = rows.Where(x => x.ReliefSoughtId == filter.ReliefSoughtId.Value);
        //    if (!string.IsNullOrEmpty(filter.LeadID))
        //        rows = rows.Where(x => x.LeadID.Contains(filter.LeadID));

        //    if (filter.MainCaseId.HasValue)
        //        rows = rows.Where(x => x.MainCaseId == filter.MainCaseId.Value);

        //    var totalItems = await rows.CountAsync();

        //    if (totalItems <= 0)
        //    {
        //        result.Title = "No Data Found";
        //        return StatusCode(result.Status, result);
        //    }

        //    // Ordering
        //    if (string.IsNullOrWhiteSpace(filter.SortBy))
        //    {
        //        filter.SortBy = "Id";
        //        filter.IsAscending = false;
        //    }

        //    rows = filter.SortBy switch
        //    {
        //        "Claimant" => filter.IsAscending ? rows.OrderBy(x => x.Claimant) : rows.OrderByDescending(x => x.Claimant),
        //        "Defendant" => filter.IsAscending ? rows.OrderBy(x => x.Defendant) : rows.OrderByDescending(x => x.Defendant),
        //        "CreatedOn" => filter.IsAscending ? rows.OrderBy(x => x.CreatedOn) : rows.OrderByDescending(x => x.CreatedOn),
        //        _ => filter.IsAscending ? rows.OrderBy(x => x.Id) : rows.OrderByDescending(x => x.Id),
        //    };

        //    // Paging
        //    result.Data.Count = totalItems;

        //    if (filter.Size > 0)
        //    {
        //        rows = rows.Skip(filter.Index * filter.Size).Take(filter.Size);
        //    }

        //    // Load workflow steps
        //    //var workflows = await unitOfWork.WorkflowRepository
        //    //    .GetAllQuerable()
        //    //    .Include(x => x.CaseStatus)
        //    //    .ToListAsync();

        //    // Projection
        //    var data = await rows.Select(x => new
        //    {
        //        Case = x,
        //    }).ToListAsync();

        //    var caseIds = data.Select(x => x.Case.Id).ToList();

        //    var activeTaskCounts = await unitOfWork.CaseTaskRepository
        //        .GetAllQuerable()
        //        .AsNoTracking()
        //        .Where(task =>
        //            caseIds.Contains(task.CaseId) &&
        //            !task.IsClosed)
        //        .GroupBy(task => task.CaseId)
        //        .Select(group => new
        //        {
        //            CaseId = group.Key,
        //            TotalActiveTasks = group.Count()
        //        })
        //        .ToDictionaryAsync(
        //            x => x.CaseId,
        //            x => x.TotalActiveTasks);



        //    result.Data.Rows = data.Select(x =>
        //    {
        //        //var currentWorkflow = workflows
        //        //    .Where(w => w.CaseTypeId == x.Case.TypeId)
        //        //    .OrderBy(w => w.Order)
        //        //    .ToList();

        //        //var currentStep = currentWorkflow
        //        //    .FirstOrDefault(w => w.CaseStatusId == x.Case.StatusId);

        //        //var nextStep = currentStep == null
        //        //    ? null
        //        //    : currentWorkflow.FirstOrDefault(w => w.Order == currentStep.Order + 1);

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

        //            //Level = new SummaryView
        //            //{
        //            //    Id = x.Case.Level!.Id,
        //            //    Name = x.Case.Level.NameEN
        //            //},

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

        //            //NextStatus = nextStep != null
        //            //    ? new SummaryView
        //            //    {
        //            //        Id = nextStep.CaseStatusId,
        //            //        Name = nextStep.CaseStatus!.NameEN
        //            //    }
        //            //    : null,

        //            ReliefSought = new SummaryView
        //            {
        //                Id = x.Case.ReliefSought!.Id,
        //                Name = x.Case.ReliefSought.NameEN
        //            },
        //            ExpertWitness=x.Case.ExpertWitnessNameEn,
        //            //ExpertWitness = x.Case.ExpertWitnessId.HasValue
        //            //    ? new SummaryView
        //            //    {
        //            //        Id = x.Case.ExpertWitnessId.Value,
        //            //        Name = x.Case.ExpertWitnessNameEn ?? string.Empty
        //            //    }
        //            //    : null,

        //            IsCompleted = x.Case.IsCompleted,

        //            CreatedOn = x.Case.CreatedOn,
        //            CreatedById = x.Case.CreatedById,
        //            CreatedByName = x.Case.CreatedByName,

        //            UpdatedOn = x.Case.UpdatedOn,
        //            UpdatedById = x.Case.UpdatedById,
        //            UpdatedByName = x.Case.UpdatedByName,
        //            CaseName=x.Case.CaseName,
        //            ProjectCode=x.Case.ProjectCode,
        //            ProjectName =x.Case.ProjectName,
        //            UnitNumber =x.Case.UnitNumber,
        //            UnitType =x.Case.UnitType,
        //            LeadStatus =x.Case.LeadStatus,
        //            BuyerName =x.Case.BuyerName,
        //            BuyerNumber =x.Case.BuyerNumber,
        //            JointBuyerName =x.Case.JointBuyerName,
        //            JointBuyerMobile =x.Case.JointBuyerMobile,
        //            SoldPrice =x.Case.SoldPrice,
        //            LeadID =x.Case.LeadID,
        //            ClosedDate = x.Case.ClosedDate,
        //            ClosedStatus = x.Case.ClosedStatus,
        //            IsClaimant=x.Case.IsClaimant,
        //            MainCase = x.Case.MainCase == null ? null : new SummaryView
        //                         {
        //                         Id = (int)x.Case.MainCase.Id,
        //                         Name = x.Case.MainCase.CaseName
        //                         },
        //            AssignedUser = x.Case.AssignedUser == null ? null : new SummaryUserView
        //            {
        //                Id = (int)x.Case.AssignedUser.Id,
        //                Name = x.Case.AssignedUser.NameEn,
        //                UserId = (int)x.Case.AssignedUser.RefId,
        //            },
        //            TotalActiveTasks = activeTaskCounts.TryGetValue( x.Case.Id,out var totalActiveTasks) ? totalActiveTasks: 0,
        //            Note=x.Case.Note,
        //            CloseReason = x.Case.CloseReason
        //           };

        //            }).ToList();




        //    return StatusCode(result.Status, result);
        //}

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
                //.Include(x => x.Level)
                .Include(x => x.Court)
                .Include(x => x.Status)
                .Include(x => x.ReliefSought)
                .Include(x => x.Notes)
                .Include(e => e.Team)
                .Include(e => e.MainCase)
                .Include(e => e.AssignedUser)
                //.Where(e => currentUser.IsSuperAdmin || e.Team.Select(t => t.UserId).Contains(currentUser.UserId))
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

                //Level = new SummaryView
                //{
                //    Id = entity.Level!.Id,
                //    Name = entity.Level.NameEN
                //},

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
                AssignedUser = entity.AssignedUser == null ? null : new SummaryUserView
                {
                    Id = (int)entity.AssignedUser.Id,
                    Name = entity.AssignedUser.NameEn,
                    UserId = (int)entity.AssignedUser.RefId,
                },
                
                Note=entity.Note,
                CloseReason = entity.CloseReason,
                ClosedDate= entity.ClosedDate
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

            //var casename = await unitOfWork.CaseRepository.GetAllQuerable().Where(x => x.CaseName == model.CaseName).FirstOrDefaultAsync();
            //if (casename is not null)
            //{
            //    result.Status = (int)ResponseEnum.Failed;
            //    result.Title = "you have another case by the same name";
            //    return StatusCode(result.Status, result);
            //}

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
                //LevelId = model.LevelId,
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
                JointBuyerName =model.JointBuyerName,
                JointBuyerMobile =model.JointBuyerMobile,
                SoldPrice =model.SoldPrice,
                AssignedUserId=model.AssignedUserId,
                Note=model.Note,
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
            if(entity.AssignedUserId != currentUser.UserId && !currentUser.IsSuperAdmin)
            {
                return StatusCode(result.Status, result);

            }


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

            //if (model.LevelId.HasValue &&
            //    entity.LevelId != model.LevelId.Value)
            //{
            //    AddAuditLog("Level", refs.CaseLevels.FirstOrDefault(r => r.Id == entity.LevelId)?.NameEN, refs.CaseLevels.FirstOrDefault(r => r.Id == model.LevelId.Value)?.NameEN);
            //    entity.LevelId = model.LevelId.Value;
            //    auditLogs.Add(new TblAuditLog
            //    {
            //        UserId = currentUser.Email,
            //        Type = "update",
            //        TableName = "case",
            //        ActionType = "update",
            //        DateTime = DateTime.Now,
            //        OldValues = entity.LevelId.ToString(),
            //        NewValues = model.LevelId.ToString(),
            //        AffectedColumns = null,
            //        PrimaryKey = json,
            //        IsArchived = false,

            //    });
            //}

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


            entity.AssignedUserId = model.AssignedUserId;
            entity.IsClaimant = model.IsClaimant;
            entity.Claimant = model.Claimant;
            entity.Note = model.Note;
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
                entity.CloseReason = null;
            }
            else
            {
                entity.ClosedStatus = (CASEENUM)model.ClosedStatus;
                entity.ClosedDate = DateOnly.FromDateTime(DateTime.Now);
                entity.CloseReason = model.CloseReason;

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

        [HttpPost("{id}/Transfer")]
        [RequiredPermission("cases.update.status")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TransferUserAsync(long id,[FromBody] TransferUserRequest model)
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

            var oldAssignUserId = entity.AssignedUserId;
            if (entity == null)
                return StatusCode(result.Status, result);

            if(model.AssignUserId == 0 )
            {
                return StatusCode(result.Status, result);

            }
            else
            {
                entity.AssignedUserId = model.AssignUserId;
            }
            var updatedOn = DateTime.Now;

           
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
                OldValues = oldAssignUserId.ToString(),
                NewValues = entity.AssignedUserId.ToString(),
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
                //.Include(e => e.Level)
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

            //if (filter.LevelId.HasValue)
            //    rows = rows.Where(x => x.LevelId == filter.LevelId.Value);

            if (filter.CourtId.HasValue)
                rows = rows.Where(x => x.CourtId == filter.CourtId.Value);

            if (filter.StatusId.HasValue)
                rows = rows.Where(x => x.StatusId == filter.StatusId.Value);

            if (filter.ReliefSoughtId.HasValue)
                rows = rows.Where(x => x.ReliefSoughtId == filter.ReliefSoughtId.Value);
           
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
                    //worksheet.Cell(row, 6).Value = c.Level?.NameEN;
                    worksheet.Cell(row, 6).Value = c.Court?.NameEN;
                    worksheet.Cell(row, 7).Value = c.Status?.NameEN;
                    //worksheet.Cell(row, 9).Value = nextStep?.CaseStatus?.NameEN;
                    worksheet.Cell(row, 8).Value = c.ReliefSought?.NameEN;
                    worksheet.Cell(row, 9).Value = c.ClaimValue;
                    worksheet.Cell(row, 10).Value = c.ProjectName;
                    worksheet.Cell(row, 11).Value = c.UnitCode;
                    worksheet.Cell(row, 12).Value = c.CreatedByName;
                    worksheet.Cell(row, 13).Value = c.CreatedOn;

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
                //.Include(e => e.Level)
                .Include(e => e.Court)
                .Include(e => e.Status)
                .Include(e => e.ReliefSought)
                .Include(e => e.Team)
                .Include(e => e.MainCase)
                .Include(e => e.AssignedUser)
                //.Where(e => (currentUser.IsSuperAdmin || e.CreatedById==currentUser.UserId) )
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

                    //Level = new SummaryView
                    //{
                    //    Id = x.Case.Level!.Id,
                    //    Name = x.Case.Level.NameEN
                    //},

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
                    IsClaimant = x.Case.IsClaimant,
                    MainCase = x.Case.MainCase == null ? null : new SummaryView
                    {
                        Id = (int)x.Case.MainCase.Id,
                        Name = x.Case.MainCase.CaseName
                    },
                    AssignedUser = x.Case.AssignedUser == null ? null : new SummaryUserView
                    {
                        Id = (int)x.Case.AssignedUser.Id,
                        Name = x.Case.AssignedUser.NameEn,
                        UserId = (int)x.Case.AssignedUser.RefId,
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
                //.Include(e => e.Level)
                .Include(e => e.Court)
                .Include(e => e.Status)
                .Include(e => e.ReliefSought)
                .Include(e => e.Team)
                .Include(e => e.MainCase)
                .Include(e => e.AssignedUser)
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
