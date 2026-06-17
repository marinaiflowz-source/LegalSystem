using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class CaseRepository:  Repository<TblCase>
    {
        public CaseRepository(ApplicationDbContext context) : base(context) { }
    }
}
