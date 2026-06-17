using LegalSystem.Attributes;
using LegalSystem.DataAccess;
using LegalSystem.DTOs;
using LegalSystem.Enums;
using LegalSystem.Helpers;
using LegalSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static LegalSystem.DTOs.AuditEntry;

namespace LegalSystem.Controllers
{
    [Route("dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ReferancesService referancesService;
        private readonly HttpContextProvider httpContext;

        public DashboardController(UnitOfWork unitOfWork, HttpContextProvider httpContext, ReferancesService referancesService)
        {
            this.unitOfWork = unitOfWork;
            this.httpContext = httpContext;
            this.referancesService = referancesService;
        }

        //[HttpGet]
        //[RequiredPermission("dashboard")]
        //[ProducesResponseType(typeof(QueryResult<CaseQuery>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        //public async Task<IActionResult> GetAsync([FromQuery] BaseRefFilter filter)
        //{
        //    var result = new Response<DashboardDto>()
        //    {
        //        Data = null,
        //        Status = (int)ResponseEnum.NotFound,
        //        Title = "Not Found"
        //    };

        //    var rows = unitOfWork.CaseRepository.GetAllQuerable()
        //        .AsNoTracking();

        //    var dashboard = new DashboardDto();
        //    dashboard.CaseStatistics = new CaseStatisticsDto
        //    {
        //        OpenCases = await rows.CountAsync(x => x.IsCompleted == false),
        //        ClosedCases = await rows.CountAsync(x => x.IsCompleted == true),
        //        ClosedCasesToday = await rows.CountAsync(x => x.IsCompleted == true && x.ClosedDate == DateOnly.FromDateTime(DateTime.Now)),
        //        WonCount = await rows.CountAsync(x => x.ClosedStatus == CASEENUM.WON),
        //        LostCount = await rows.CountAsync(x => x.ClosedStatus == CASEENUM.LOST),
        //        SettledCount = await rows.CountAsync(x => x.ClosedStatus == CASEENUM.SETTLED)
        //    };


        //    dashboard.TeamPerformance = await (from ct in unitOfWork.CaseTeamRepository.GetAllQuerable().AsNoTracking()
        //                        join u in unitOfWork.UserRepository.GetAllQuerable().AsNoTracking()
        //                            on ct.UserId equals u.Id
        //                        where ct.Case.IsCompleted == true
        //                        group new { ct, u } by new { ct.UserId, u.NameEn } into g
        //                        select new TeamPerformanceDto
        //                        {
        //                            UserName = g.Key.NameEn,
        //                            TotalClosedCases = g.Select(x => x.ct.CaseId).Distinct().Count(),
        //                            WonCases = g.Count(x => x.ct.Case.ClosedStatus == CASEENUM.WON),
        //                            WinPercentage = g.Select(x => x.ct.CaseId).Distinct().Count() == 0
        //                                ? 0
        //                                : (decimal)g.Count(x => x.ct.Case.ClosedStatus == CASEENUM.WON)
        //                                  * 100 / g.Select(x => x.ct.CaseId).Distinct().Count()
        //                        })
        //                .OrderByDescending(x => x.WinPercentage)
        //                .ToListAsync();


        //    dashboard.LitigationDensity = await rows
        //                  .AsNoTracking()
        //                  .GroupBy(x => new
        //                  {
        //                     x.UnitCode,
        //                     x.ProjectName
        //                  })
        //                  .Select(g => new LitigationDensityDto
        //                  {
        //                    UnitCode = g.Key.UnitCode,
        //                    ProjectName = g.Key.ProjectName,
        //                    CaseCount = g.Count()
        //                  })
        //                  .OrderByDescending(x => x.CaseCount)
        //                  .ToListAsync();

        //    dashboard.ProjectOutcome = await unitOfWork.CaseRepository.GetAllQuerable()
        //                .AsNoTracking()
        //                .Where(x => x.IsCompleted)
        //                .GroupBy(x => new
        //                {
        //                  x.ProjectCode,
        //                  x.ProjectName
        //                })
        //                .Select(g => new ProjectOutcomeDto
        //                {
        //                   ProjectCode = g.Key.ProjectCode,
        //                   ProjectName = g.Key.ProjectName,

        //                   WonCases = g.Count(x => x.ClosedStatus == CASEENUM.WON),

        //                   TotalClosedCases = g.Count(),

        //                   SuccessRate = g.Count() == 0
        //                                 ? 0
        //                                 : Math.Round(
        //                                 (decimal)g.Count(x => x.ClosedStatus == CASEENUM.WON) * 100
        //                                  / g.Count(),
        //                                 2)
        //                                 })
        //                .OrderByDescending(x => x.SuccessRate)
        //                .ToListAsync();

        //    dashboard.ActiveWorkflow = await unitOfWork.CaseTaskRepository
        //                               .GetAllQuerable()
        //                               .AsNoTracking()
        //                               .Where(x => !x.IsClosed)
        //                               .OrderBy(x => x.DueDate)
        //                               .Select(x => new ActiveWorkflowDto
        //                               {
        //                                TaskId = (int)x.Id,
        //                                Title = x.Title,
        //                                DueDate = x.DueDate,
        //                                PriorityId = x.PriorityId,
        //                                StatusId = x.StatusId
        //                               })
        //                               .Take(10)
        //                               .ToListAsync();

        //    var currentYear = DateTime.Now.Year;



        //   var data = await unitOfWork.CaseRepository
        //              .GetAllQuerable()
        //              .AsNoTracking()
        //              .Where(x => x.CreatedOn.Year == currentYear)
        //              .GroupBy(x => x.CreatedOn.Month)
        //              .Select(g => new
        //              {
        //                 MonthNumber = g.Key,
        //                 TotalFilings = g.Count(),
        //                 ClosedTotal = g.Count(x => x.IsCompleted),
        //                 WonCases = g.Count(x =>
        //                 x.IsCompleted &&
        //                 x.ClosedStatus == CASEENUM.WON)
        //              })
        //              .OrderBy(x => x.MonthNumber)
        //              .ToListAsync();

        //    var currentMonth = DateTime.Now.Month;
        //    //var currentYear = DateTime.Now.Year;

        //    var months = Enumerable.Range(currentMonth - 3, 7)
        //        .Select(m =>
        //        {
        //            var date = new DateTime(currentYear, 1, 1).AddMonths(m - 1);

        //            return new MonthlyCaseStatisticsDto
        //            {
        //                Month = date.ToString("MMM"),
        //                TotalFilings = 0,
        //                ClosedTotal = 0,
        //                WonCases = 0
        //            };
        //        })
        //        .ToList();

        //    foreach (var month in months)
        //    {
        //        var dbMonth = data.FirstOrDefault(x =>
        //            CultureInfo.InvariantCulture.DateTimeFormat
        //                .AbbreviatedMonthNames[x.MonthNumber - 1] == month.Month);

        //        if (dbMonth != null)
        //        {
        //            month.TotalFilings = dbMonth.TotalFilings;
        //            month.ClosedTotal = dbMonth.ClosedTotal;
        //            month.WonCases = dbMonth.WonCases;
        //        }
        //    }

        //    dashboard.MonthlyCaseStatistics = months;

        //    result.Status = (int)ResponseEnum.Succeeded;
        //    result.Title = "Data Retrieved";
        //    result.Data = dashboard;

        //    return StatusCode(result.Status, result);
        //}

        [HttpGet]
        [RequiredPermission("dashboard")]
        [ProducesResponseType(typeof(QueryResult<CaseQuery>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetAsync([FromQuery] BaseRefFilter filter)
        {
            var result = new Response<DashboardDto>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.CaseRepository.GetAllQuerable()
                .AsNoTracking();

            var dashboard = new DashboardDto();
            dashboard.CaseStatistics = new CaseStatisticsDto
            {
                OpenCases = await rows.CountAsync(x => x.IsCompleted == false),
                ClosedCases = await rows.CountAsync(x => x.IsCompleted == true),
                ClosedCasesToday = await rows.CountAsync(x => x.IsCompleted == true && x.ClosedDate == DateOnly.FromDateTime(DateTime.Now)),
                WonCount = await rows.CountAsync(x => x.ClosedStatus == CASEENUM.WON),
                LostCount = await rows.CountAsync(x => x.ClosedStatus == CASEENUM.LOST),
                SettledCount = await rows.CountAsync(x => x.ClosedStatus == CASEENUM.SETTLED)
            };


            dashboard.TeamPerformance = await (from ct in unitOfWork.CaseTeamRepository.GetAllQuerable().AsNoTracking()
                                               join u in unitOfWork.UserRepository.GetAllQuerable().AsNoTracking()
                                                   on ct.UserId equals u.Id
                                               where ct.Case.IsCompleted == true
                                               group new { ct, u } by new { ct.UserId, u.NameEn } into g
                                               select new TeamPerformanceDto
                                               {
                                                   UserName = g.Key.NameEn,
                                                   TotalClosedCases = g.Select(x => x.ct.CaseId).Distinct().Count(),
                                                   WonCases = g.Count(x => x.ct.Case.ClosedStatus == CASEENUM.WON),
                                                   WinPercentage = g.Select(x => x.ct.CaseId).Distinct().Count() == 0
                                                       ? 0
                                                       : (decimal)g.Count(x => x.ct.Case.ClosedStatus == CASEENUM.WON)
                                                         * 100 / g.Select(x => x.ct.CaseId).Distinct().Count()
                                               })
                        .OrderByDescending(x => x.WinPercentage)
                        .ToListAsync();


            dashboard.LitigationDensity = await rows
                          .AsNoTracking()
                          .GroupBy(x => new
                          {
                              x.UnitCode,
                              x.ProjectName
                          })
                          .Select(g => new LitigationDensityDto
                          {
                              UnitCode = g.Key.UnitCode,
                              ProjectName = g.Key.ProjectName,
                              CaseCount = g.Count()
                          })
                          .OrderByDescending(x => x.CaseCount)
                          .ToListAsync();

            dashboard.ProjectOutcome = await unitOfWork.CaseRepository.GetAllQuerable()
                        .AsNoTracking()
                        .Where(x => x.IsCompleted)
                        .GroupBy(x => new
                        {
                            x.ProjectCode,
                            x.ProjectName
                        })
                        .Select(g => new ProjectOutcomeDto
                        {
                            ProjectCode = g.Key.ProjectCode,
                            ProjectName = g.Key.ProjectName,

                            WonCases = g.Count(x => x.ClosedStatus == CASEENUM.WON),

                            TotalClosedCases = g.Count(),

                            SuccessRate = g.Count() == 0
                                         ? 0
                                         : Math.Round(
                                         (decimal)g.Count(x => x.ClosedStatus == CASEENUM.WON) * 100
                                          / g.Count(),
                                         2)
                        })
                        .OrderByDescending(x => x.SuccessRate)
                        .ToListAsync();

            dashboard.ActiveWorkflow = await unitOfWork.CaseTaskRepository
                                       .GetAllQuerable()
                                       .AsNoTracking()
                                       .Where(x => !x.IsClosed)
                                       .OrderBy(x => x.DueDate)
                                       .Select(x => new ActiveWorkflowDto
                                       {
                                           TaskId = (int)x.Id,
                                           Title = x.Title,
                                           DueDate = x.DueDate,
                                           PriorityId = x.PriorityId,
                                           StatusId = x.StatusId
                                       })
                                       .Take(10)
                                       .ToListAsync();

            var currentYear = DateTime.Now.Year;



            var data = await unitOfWork.CaseRepository
                       .GetAllQuerable()
                       .AsNoTracking()
                       .Where(x => x.CreatedOn.Year == currentYear)
                       .GroupBy(x => x.CreatedOn.Month)
                       .Select(g => new
                       {
                           MonthNumber = g.Key,
                           TotalFilings = g.Count(),
                           ClosedTotal = g.Count(x => x.IsCompleted),
                           WonCases = g.Count(x =>
                          x.IsCompleted &&
                          x.ClosedStatus == CASEENUM.WON)
                       })
                       .OrderBy(x => x.MonthNumber)
                       .ToListAsync();

            var currentMonth = DateTime.Now.Month;
            //var currentYear = DateTime.Now.Year;

            var months = Enumerable.Range(currentMonth - 3, 7)
                .Select(m =>
                {
                    var date = new DateTime(currentYear, 1, 1).AddMonths(m - 1);

                    return new MonthlyCaseStatisticsDto
                    {
                        Month = date.ToString("MMM"),
                        TotalFilings = 0,
                        ClosedTotal = 0,
                        WonCases = 0
                    };
                })
                .ToList();

            foreach (var month in months)
            {
                var dbMonth = data.FirstOrDefault(x =>
                    CultureInfo.InvariantCulture.DateTimeFormat
                        .AbbreviatedMonthNames[x.MonthNumber - 1] == month.Month);

                if (dbMonth != null)
                {
                    month.TotalFilings = dbMonth.TotalFilings;
                    month.ClosedTotal = dbMonth.ClosedTotal;
                    month.WonCases = dbMonth.WonCases;
                }
            }

            dashboard.MonthlyCaseStatistics = months;

            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = dashboard;

            return StatusCode(result.Status, result);
        }


    }
}
