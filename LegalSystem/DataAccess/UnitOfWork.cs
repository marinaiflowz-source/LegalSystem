using LegalSystem.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LegalSystem.DataAccess
{
    public class UnitOfWork
    {
        private readonly ApplicationDbContext dbContext;

        // Def
        private AuditLogRepository? auditLogRepository;
        private ReasonRepository? reasonRepository;
        private CaseRepository? caseRepository;
        private CaseEventRepository? caseEventRepository;
        private CaseDocumentRepository? caseDocumentRepository;
        private CaseTaskRepository? caseTaskRepository;
        private CaseTeamRepository? caseTeamRepository;
        private CaseNoteRepository? caseNoteRepository;
        private CaseAuditLogRepository? caseAuditLogRepository;
        private UserRepository? userRepository;
        private WorkflowRepository? workflowRepository;

        private RefTaskTypeRepository? refTaskTypeRepository;
        private RefCaseLevelRepository? refCaseLevelRepository;
        private RefCaseStatusRepository? refCaseStatusRepository;
        private RefCaseTypeRepository? refCaseTypeRepository;
        private RefCourtRepository? refCourtRepository;
        private RefReliefSoughtRepository? refReliefSoughtRepository;
        private RefUserTypeRepository? refUserTypeRepository;
        private RefTaskPriorityRepository? refTaskPriorityRepository;
        private RefTaskStatusRepository? refTaskStatusRepository;
        private RefDocumentClassificationRepository? refDocumentClassificationRepository;
        private RefEventTypeRepository? refEventTypeRepository;

        //Constracture
        public UnitOfWork(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        public AuditLogRepository AuditLogRepository
        {
            get
            {
                auditLogRepository ??= new AuditLogRepository(dbContext);
                return auditLogRepository!;
            }
        }

        public ReasonRepository ReasonRepository
        {
            get
            {
                reasonRepository ??= new ReasonRepository(dbContext);
                return reasonRepository!;
            }
        }

        // Assign
        public CaseRepository CaseRepository
        {
            get
            {
                caseRepository ??= new CaseRepository(dbContext);
                return caseRepository!;
            }
        }
        public CaseEventRepository CaseEventRepository
        {
            get
            {
                caseEventRepository ??= new CaseEventRepository(dbContext);
                return caseEventRepository!;
            }
        }
        public CaseDocumentRepository CaseDocumentRepository
        {
            get
            {
                caseDocumentRepository ??= new CaseDocumentRepository(dbContext);
                return caseDocumentRepository!;
            }
        }
        public CaseTaskRepository CaseTaskRepository
        {
            get
            {
                caseTaskRepository ??= new CaseTaskRepository(dbContext);
                return caseTaskRepository!;
            }
        }
        public CaseTeamRepository CaseTeamRepository
        {
            get
            {
                caseTeamRepository ??= new CaseTeamRepository(dbContext);
                return caseTeamRepository!;
            }
        }
        public CaseNoteRepository CaseNoteRepository
        {
            get
            {
                caseNoteRepository ??= new CaseNoteRepository(dbContext);
                return caseNoteRepository!;
            }
        }
        public CaseAuditLogRepository CaseAuditLogRepository
        {
            get
            {
                caseAuditLogRepository ??= new CaseAuditLogRepository(dbContext);
                return caseAuditLogRepository!;
            }
        }
        public UserRepository UserRepository
        {
            get
            {
                userRepository ??= new UserRepository(dbContext);
                return userRepository!;
            }
        }
        public WorkflowRepository WorkflowRepository
        {
            get
            {
                workflowRepository ??= new WorkflowRepository(dbContext);
                return workflowRepository!;
            }
        }


        public RefTaskTypeRepository RefTaskTypeRepository
        {
            get
            {
                refTaskTypeRepository ??= new RefTaskTypeRepository(dbContext);
                return refTaskTypeRepository!;
            }
        }

        public RefCaseLevelRepository RefCaseLevelRepository
        {
            get
            {
                refCaseLevelRepository ??= new RefCaseLevelRepository(dbContext);
                return refCaseLevelRepository!;
            }
        }

        public RefCaseStatusRepository RefCaseStatusRepository
        {
            get
            {
                refCaseStatusRepository ??= new RefCaseStatusRepository(dbContext);
                return refCaseStatusRepository!;
            }
        }

        public RefCaseTypeRepository RefCaseTypeRepository
        {
            get
            {
                refCaseTypeRepository ??= new RefCaseTypeRepository(dbContext);
                return refCaseTypeRepository!;
            }
        }

        public RefCourtRepository RefCourtRepository
        {
            get
            {
                refCourtRepository ??= new RefCourtRepository(dbContext);
                return refCourtRepository!;
            }
        }

        public RefReliefSoughtRepository RefReliefSoughtRepository
        {
            get
            {
                refReliefSoughtRepository ??= new RefReliefSoughtRepository(dbContext);
                return refReliefSoughtRepository!;
            }
        }

        public RefUserTypeRepository RefUserTypeRepository
        {
            get
            {
                refUserTypeRepository ??= new RefUserTypeRepository(dbContext);
                return refUserTypeRepository!;
            }
        }
        
        public RefTaskPriorityRepository RefTaskPriorityRepository
        {
            get
            {
                refTaskPriorityRepository ??= new RefTaskPriorityRepository(dbContext);
                return refTaskPriorityRepository!;
            }
        }

        public RefTaskStatusRepository RefTaskStatusRepository
        {
            get
            {
                refTaskStatusRepository ??= new RefTaskStatusRepository(dbContext);
                return refTaskStatusRepository!;
            }
        }

        public RefDocumentClassificationRepository RefDocumentClassificationRepository
        {
            get
            {
                refDocumentClassificationRepository ??= new RefDocumentClassificationRepository(dbContext);
                return refDocumentClassificationRepository!;
            }
        }

        public RefEventTypeRepository RefEventTypeRepository
        {
            get
            {
                refEventTypeRepository ??= new RefEventTypeRepository(dbContext);
                return refEventTypeRepository!;
            }
        }

        // Transactions
        public Task<int> CompleteAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return dbContext.SaveChangesAsync(cancellationToken);
        }
        public async Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters)
        {
            return await dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
        }
    }
}
