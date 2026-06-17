using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class RefTaskStatusRepository : Repository<RefTaskStatus>
    {
        public RefTaskStatusRepository(ApplicationDbContext context) : base(context) { }
    }
}
