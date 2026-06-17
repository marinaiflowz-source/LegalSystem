using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class RefReliefSoughtRepository : Repository<RefReliefSought>
    {
        public RefReliefSoughtRepository(ApplicationDbContext context) : base(context) { }
    }
}
