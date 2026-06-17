using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class CaseTeamRepository : Repository<TblCaseTeam>
    {
        public CaseTeamRepository(ApplicationDbContext context) : base(context) { }
    }
}
