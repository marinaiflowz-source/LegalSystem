using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class RefCaseLevelRepository : Repository<RefCaseLevel>
    {
        public RefCaseLevelRepository(ApplicationDbContext context) : base(context) { }
    }
}
