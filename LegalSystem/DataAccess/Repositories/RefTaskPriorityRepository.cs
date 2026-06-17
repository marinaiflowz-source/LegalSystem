using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class RefTaskPriorityRepository : Repository<RefTaskPriority>
    {
        public RefTaskPriorityRepository(ApplicationDbContext context) : base(context) { }
    }
}
