using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class RefCourtRepository : Repository<RefCourt>
    {
        public RefCourtRepository(ApplicationDbContext context) : base(context) { }
    }
}
