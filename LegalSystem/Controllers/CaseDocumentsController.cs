using Amazon.S3;
using Amazon.S3.Model;
using LegalSystem.Attributes;
using LegalSystem.Constants;
using LegalSystem.DataAccess;
using LegalSystem.DTOs;
using LegalSystem.Enums;
using LegalSystem.Helpers;
using LegalSystem.Models;
using LegalSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Buffers.Text;
using System.Text.Json;

namespace LegalSystem.Controllers
{
    [Route("case-documents")]
    [ApiController]
    public class CaseDocumentController : ControllerBase
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ReferancesService referancesService;
        private readonly FileStorageService fileStorageService;
        private readonly FileStorageConfiguration fileStorageConfiguration;
        private readonly HttpContextProvider httpContext;
        private readonly IAmazonS3 _s3Client;

        public CaseDocumentController(UnitOfWork unitOfWork, FileStorageService fileStorageService,
            IOptionsSnapshot<FileStorageConfiguration> fileStorageConfigurationOpt, HttpContextProvider httpContext,
            ReferancesService referancesService,
            IAmazonS3 s3Client)
        {
            this.unitOfWork = unitOfWork;
            this.fileStorageService = fileStorageService;
            fileStorageConfiguration = fileStorageConfigurationOpt.Value;
            this.httpContext = httpContext;
            this.referancesService = referancesService;
            _s3Client = s3Client;
        }

        // ========================= GET ALL =========================
        [HttpGet]
        [RequiredPermission("caseDocuments.get")]
        [ProducesResponseType(typeof(QueryResult<CaseDocumentQuery>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync([FromQuery] CaseDocumentFilter filter)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new QueryResult<CaseDocumentQuery>()
            {
                Status = (int)ResponseEnum.Succeeded,
                Title = "Data Retrieved"
            };

            var rows = unitOfWork.CaseDocumentRepository.GetAllQuerable()
                //.Include(e => e.Classification)
                .Include(e => e.Case).ThenInclude(c => c!.Team)
                .Where(e => currentUser.IsSuperAdmin || e.Case!.Team.Select(t => t.UserId).Contains(currentUser.UserId))
                .AsNoTracking();

            // Filtering
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                rows = rows.Where(x =>
                    (x.OriginalName).Contains(filter.SearchText));
            }

            //if (filter.ClassificationId.HasValue)
            //    rows = rows.Where(x => x.ClassificationId == filter.ClassificationId.Value);

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
                //"ClassificationId" => filter.IsAscending ? rows.OrderBy(x => x.ClassificationId) : rows.OrderByDescending(x => x.ClassificationId),
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
             new CaseDocumentQuery
             {
                 Id = x.Id,
                 CaseId = x.CaseId,
                 CaseName=x.Case.CaseName,
                 CaseNumber="C"+ x.CaseId,
                 //Classification = x.Classification == null ? null : new SummaryView
                 //{
                 //    Id = x.Classification.Id,
                 //    Name = x.Classification.NameEN,
                 //},

                 OriginalName = x.OriginalName,
                 FileName = x.FileName,
                 Url = x.Url,
                 SizeMB = x.SizeMB,

                 CreatedOn = x.CreatedOn,
                 CreatedById = x.CreatedById,
                 CreatedByName = x.CreatedByName,

                 UpdatedOn = x.UpdatedOn,
                 UpdatedById = x.UpdatedById,
                 UpdatedByName = x.UpdatedByName,
                 DocumentName=x.DocumentName
             }
            ).ToList();



            //var entity = new TblAuditLog
            //{
            //    UserId = currentUser.Email,
            //    Type = "GetAll",
            //    TableName = "TblCaseDocuments",
            //    ActionType="GetAll",
            //    DateTime = DateTime.Now,
            //    OldValues = null,
            //    NewValues = null,
            //    AffectedColumns = null,
            //    PrimaryKey = null,
            //    IsArchived = false,
               
            //};

            //await unitOfWork.AuditLogRepository.AddAsync(entity);

            //await unitOfWork.CompleteAsync();
            result.Data.Rows = data;
            return StatusCode(result.Status, result);
        }

        // ========================= GET BY CASE =========================
        [HttpGet("{caseId}")]
        [RequiredPermission("caseDocuments.getByCase")]
        [ProducesResponseType(typeof(Response<IEnumerable<CaseDocumentQuery>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync(long caseId)
        {
            var currentUser = await httpContext.GetCurrentUser();
            var result = new Response<IEnumerable<CaseDocumentQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var rows = unitOfWork.CaseDocumentRepository.GetAllQuerable()
                //.Include(e => e.Classification)
                .Include(e => e.Case).ThenInclude(c => c!.Team)
                .Where(e => e.CaseId == caseId)
                .Where(e => currentUser.IsSuperAdmin || e.Case!.Team.Select(t => t.UserId).Contains(currentUser.UserId))
                .AsNoTracking();

            var data = await rows.Select(x => new CaseDocumentQuery
            {
                Id = x.Id,
                CaseId = x.CaseId,

                //Classification = x.Classification == null ? null : new SummaryView
                //{
                //    Id = x.Classification.Id,
                //    Name = x.Classification.NameEN,
                //},

                OriginalName = x.OriginalName,
                FileName = x.FileName,
                Url = x.Url,
                SizeMB = x.SizeMB,
                DocumentName=x.DocumentName,
                CreatedOn = x.CreatedOn,
                CreatedById = x.CreatedById,
                CreatedByName = x.CreatedByName,

                UpdatedOn = x.UpdatedOn,
                UpdatedById = x.UpdatedById,
                UpdatedByName = x.UpdatedByName,

            }).ToListAsync();

            //var entity = new TblAuditLog
            //{
            //    UserId = currentUser.Email,
            //    Type = "GetByCase",
            //    TableName = "TblCaseDocuments",
            //    ActionType = "GetByCase",
            //    DateTime = DateTime.Now,
            //    OldValues = null,
            //    NewValues = null,
            //    AffectedColumns = null,
            //    PrimaryKey = null,
            //    IsArchived = false,

            //};

            //await unitOfWork.AuditLogRepository.AddAsync(entity);

            //await unitOfWork.CompleteAsync();
            result.Status = (int)ResponseEnum.Succeeded;
            result.Title = "Data Retrieved";
            result.Data = data;

            return StatusCode(result.Status, result);
        }

        // ========================= DOWNLOAD DOC =========================
        [HttpGet("{id}/download")]
        [RequiredPermission("caseDocuments.download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadDocumentAsync(long id)
        {
            var result = new Response<IEnumerable<CaseDocumentQuery>?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Unauthorized,
                Title = "Not Authorized"
            };
            var currentUser = await httpContext.GetCurrentUser();


            var entity = await unitOfWork.CaseDocumentRepository
                .GetAllQuerable()
                .AsNoTracking()
                .Where(e => e.Id == id)
                //.Select(e => e.FileName)
                .FirstOrDefaultAsync();

            if (entity is null)
                return StatusCode(result.Status, result);

            var caseTeamIds = unitOfWork.CaseTeamRepository.GetAllQuerable().Where(e => e.CaseId == entity.CaseId).Select(e => e.UserId);
            if (!currentUser.IsSuperAdmin && !caseTeamIds.Contains(currentUser.UserId))
                return StatusCode(result.Status, result);

            var fileName = entity.FileName;
            if (string.IsNullOrWhiteSpace(fileName))
                return StatusCode(result.Status, result);

            var file = await fileStorageService.DownloadFileAsync(fileName);

            if (file == null || file.Stream == null)
                return NotFound();

            file.Stream.Position = 0;


            var entityDocument = new TblAuditLog
            {
                UserId = currentUser.Email,
                Type = "download",
                TableName = "Documents",
                ActionType = "download",
                DateTime = DateTime.Now,
                OldValues = null,
                NewValues = fileName,
                AffectedColumns = null,
                PrimaryKey = null,
                IsArchived = false,

            };

            await unitOfWork.AuditLogRepository.AddAsync(entityDocument);

            await unitOfWork.CompleteAsync();
            return File(
                file.Stream,
                file.MimeType,
                file.Name
            );
        }

        // ========================= CREATE =========================
        [HttpPost("create")]
        [RequiredPermission("caseDocuments.create")]
        [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAsync([FromForm] CaseDocumentCommand model)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<long?>()
            {
                Data = null,
                Status = (int)ResponseEnum.Created,
                Title = "Created"
            };

            var caseTeamIds = unitOfWork.CaseTeamRepository.GetAllQuerable().Where(e => e.CaseId == model.CaseId).Select(e => e.UserId);
            if (!currentUser.IsSuperAdmin && !caseTeamIds.Contains(currentUser.UserId))
            {
                result.Status = (int)ResponseEnum.Unauthorized;
                result.Title = "Not authorized";
                return StatusCode(result.Status, result);
            }

            var baseUrl = FileHelper.GetBaseUrl(
                fileStorageConfiguration.Host,
                fileStorageConfiguration.Port,
                fileStorageConfiguration.MainBucket);

            var fileName = await fileStorageService.UploadFileAsync(model.File, false);

            baseUrl = baseUrl + fileName;

            var entity = new TblCaseDocument
            {
                CaseId = model.CaseId,
                //ClassificationId = model.ClassificationId,
                OriginalName = model.File.FileName,
                FileName = fileName,
                Url = baseUrl,
                SizeMB = (double)model.File.Length / (1024 * 1024),
                DocumentName=model.DocumentName,
                CreatedOn = DateTime.Now,
                CreatedById = currentUser.UserId,
                CreatedByName = currentUser.UserName,
            };

            await unitOfWork.CaseDocumentRepository.AddAsync(entity);
            await unitOfWork.CaseAuditLogRepository.AddAsync(new TblCaseAuditLog
            {
                CaseId = model.CaseId,
                Comment = $"Case document uploaded. File: {model.File.FileName}, ClassificationId: {model.ClassificationId}",
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
                TableName = "Documents",
                ActionType = "Create",
                DateTime = DateTime.Now,
                OldValues = null,
                NewValues = fileName,
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
        [RequiredPermission("caseDocuments.update")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] CaseDocumentUpdate model)
        {
            var currentUser = await httpContext.GetCurrentUser();

            var result = new Response<bool>()
            {
                Data = false,
                Status = (int)ResponseEnum.NotFound,
                Title = "Not Found"
            };

            var entity = await unitOfWork.CaseDocumentRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

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
                    Comment = $"Document {fieldName} updated from {oldValue ?? "NULL"} to {newValue ?? "NULL"}",
                    CreatedOn = updatedOn,
                    CreatedById = currentUser.UserId,
                    CreatedByName = currentUser.UserName,
                });

                auditLogs.AddRange(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "Documents",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = oldValue.ToString(),
                    NewValues = newValue.ToString(),
                    AffectedColumns = null,
                    PrimaryKey =null,
                    IsArchived = false,

                });

                
            }

            //if (model.ClassificationId.HasValue &&
            //    entity.ClassificationId != model.ClassificationId.Value)
            //{
            //    AddAuditLog("Classification", refs.DocumentClassifications.FirstOrDefault(r => r.Id == entity.ClassificationId)?.NameEN, refs.DocumentClassifications.FirstOrDefault(r => r.Id == model.ClassificationId.Value)?.NameEN);
            //    entity.ClassificationId = model.ClassificationId.Value;
            //    auditLogs.AddRange(new TblAuditLog
            //    {
            //        UserId = currentUser.Email,
            //        Type = "update",
            //        TableName = "CaseDocument",
            //        ActionType = "update",
            //        DateTime = DateTime.Now,
            //        OldValues = entity.ClassificationId.ToString(),
            //        NewValues = model.ClassificationId.Value.ToString(),
            //        AffectedColumns = null,
            //        PrimaryKey = null,
            //        IsArchived = false,

            //    });
            //}

            if (!string.IsNullOrWhiteSpace(model.DocumentName) &&
                entity.DocumentName != model.DocumentName)
            {
                AddAuditLog("DocumentName", entity.DocumentName, model.DocumentName);
                entity.DocumentName = model.DocumentName;
                auditLogs.AddRange(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "CaseDocument",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.DocumentName.ToString(),
                    NewValues = model.DocumentName.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = null,
                    IsArchived = false,

                });
            }
            if (!string.IsNullOrWhiteSpace(model.OriginalName) &&
                entity.OriginalName != model.OriginalName)
            {
                AddAuditLog("OriginalName", entity.OriginalName, model.OriginalName);
                entity.OriginalName = model.OriginalName;
                auditLogs.AddRange(new TblAuditLog
                {
                    UserId = currentUser.Email,
                    Type = "update",
                    TableName = "CaseDocument",
                    ActionType = "update",
                    DateTime = DateTime.Now,
                    OldValues = entity.OriginalName.ToString(),
                    NewValues = model.OriginalName.ToString(),
                    AffectedColumns = null,
                    PrimaryKey = null,
                    IsArchived = false,

                });
            }

            entity.UpdatedOn = updatedOn;
            entity.UpdatedById = currentUser.UserId;
            entity.UpdatedByName = currentUser.UserName;

            unitOfWork.CaseDocumentRepository.Update(entity);

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
        [RequiredPermission("caseDocuments.delete")]
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

            var entity = await unitOfWork.CaseDocumentRepository.GetAllQuerable()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return StatusCode(result.Status, result);

            await unitOfWork.CaseAuditLogRepository.AddAsync(new TblCaseAuditLog
            {
                CaseId = entity.CaseId,
                Comment = $"Case document deleted. File: {entity.OriginalName}",
                CreatedOn = DateTime.Now,
                CreatedById = currentUser.UserId,
                CreatedByName = currentUser.UserName,
            });

            unitOfWork.CaseDocumentRepository.Delete(entity);

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
                TableName = "Documents",
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


        //private async Task<double> GetBucketStorageGBAsync()
        //{
        //    var bucketName = fileStorageConfiguration.MainBucket;

        //    double totalSize = 0;

        //    var request = new ListObjectsV2Request
        //    {
        //        BucketName = bucketName,
        //        MaxKeys = 1000
        //    };

        //    ListObjectsV2Response response;

        //    do
        //    {
        //        response = await _s3Client.ListObjectsV2Async(request);

        //        totalSize += response.S3Objects?.Sum(x => x.Size) ?? 0;

        //        request.ContinuationToken = response.NextContinuationToken;

        //    } while (response.IsTruncated == true);

        //    return totalSize / (1024 * 1024 * 1024);
        //}

        //[HttpGet("storage")]
        //public async Task<IActionResult> GetStorageAsync()
        //{
        //    var usedGb = await GetBucketStorageGBAsync();

        //    return Ok(new
        //    {
        //        Bucket = fileStorageConfiguration.MainBucket,
        //        UsedGB = Math.Round(usedGb, 2),
        //        TotalGB = 50,
        //        UsagePercent = Math.Round((usedGb / 50) * 100, 2)
        //    });
        //}


        //[HttpGet("storage")]
        //[RequiredPermission("caseDocuments.read")]
        //[ProducesResponseType(typeof(Response<CaseStorageDto>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> GetCaseStorageAsync()
        //{
        //    var currentUser = await httpContext.GetCurrentUser();

        //    var caseTeamIds = unitOfWork.CaseTeamRepository
        //        .GetAllQuerable()
        //        .Select(e => e.UserId);

        //    if (!currentUser.IsSuperAdmin && !caseTeamIds.Contains(currentUser.UserId))
        //    {
        //        return StatusCode((int)ResponseEnum.Unauthorized, new Response<CaseStorageDto>
        //        {
        //            Data = null,
        //            Status = (int)ResponseEnum.Unauthorized,
        //            Title = "Not authorized"
        //        });
        //    }

        //    var documents = unitOfWork.CaseDocumentRepository
        //        .GetAllQuerable()
        //        .Select(e => new { e.SizeMB, e.FileName, e.OriginalName, e.CreatedOn })
        //        .ToList();

        //    var storageDto = new CaseStorageDto
        //    {
        //        TotalDocuments = documents.Count,
        //        TotalSizeMB = Math.Round(documents.Sum(d => d.SizeMB), 4),
        //        TotalSizeGB = Math.Round(documents.Sum(d => d.SizeMB) / 1024, 6),
        //        Documents = documents.Select(d => new DocumentStorageItemDto
        //        {
        //            FileName = d.FileName,
        //            OriginalName = d.OriginalName,
        //            SizeMB = Math.Round(d.SizeMB, 4),
        //            CreatedOn = d.CreatedOn
        //        }).ToList()
        //    };

        //    return Ok(new Response<CaseStorageDto>
        //    {
        //        Data = storageDto,
        //        Status = (int)ResponseEnum.Succeeded,
        //        Title = "Success"
        //    });
        //}


        //public class CaseStorageDto
        //{
        //    public long CaseId { get; set; }
        //    public int TotalDocuments { get; set; }
        //    public double TotalSizeMB { get; set; }
        //    public double TotalSizeGB { get; set; }
        //    public List<DocumentStorageItemDto> Documents { get; set; } = new();
        //}

        //public class DocumentStorageItemDto
        //{
        //    public string FileName { get; set; }
        //    public string OriginalName { get; set; }
        //    public double SizeMB { get; set; }
        //    public DateTime CreatedOn { get; set; }
        //}

    }
}
