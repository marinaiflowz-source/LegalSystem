using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class CaseAuditLogRepository : Repository<TblCaseAuditLog>
    {
        public CaseAuditLogRepository(ApplicationDbContext context) : base(context) { }
    }
}
