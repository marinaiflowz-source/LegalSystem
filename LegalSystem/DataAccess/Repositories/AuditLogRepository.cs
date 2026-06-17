using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
   
    public class AuditLogRepository : Repository<TblAuditLog>
    {
        public AuditLogRepository(ApplicationDbContext context) : base(context) { }
    }
}
