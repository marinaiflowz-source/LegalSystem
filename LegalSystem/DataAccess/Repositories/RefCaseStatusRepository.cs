using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class RefCaseStatusRepository : Repository<RefCaseStatus>
    {
        public RefCaseStatusRepository(ApplicationDbContext context) : base(context) { }
    }
}
