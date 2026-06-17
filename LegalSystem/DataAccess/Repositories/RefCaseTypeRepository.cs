using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class RefCaseTypeRepository : Repository<RefCaseType>
    {
        public RefCaseTypeRepository(ApplicationDbContext context) : base(context) { }
    }
}
