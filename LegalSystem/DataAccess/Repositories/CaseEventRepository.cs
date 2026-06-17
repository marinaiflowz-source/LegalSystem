using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{

    public class CaseEventRepository : Repository<TblCaseEvent>
    {
        public CaseEventRepository(ApplicationDbContext context) : base(context) { }
    }
}
