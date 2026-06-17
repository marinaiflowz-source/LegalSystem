using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class RefEventTypeRepository : Repository<RefEventType>
    {
        public RefEventTypeRepository(ApplicationDbContext context) : base(context) { }
    }
}
