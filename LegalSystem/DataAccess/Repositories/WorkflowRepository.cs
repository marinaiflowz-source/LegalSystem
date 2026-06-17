using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class WorkflowRepository : Repository<TblWorkFlow>
    {
        public WorkflowRepository(ApplicationDbContext context) : base(context) { }
    }
}
