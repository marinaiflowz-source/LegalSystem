using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class CaseTaskRepository : Repository<TblCaseTask>
    {
        public CaseTaskRepository(ApplicationDbContext context) : base(context) { }
    }
}
