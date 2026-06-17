using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class ReasonRepository : Repository<Reason>
    {
        public ReasonRepository(ApplicationDbContext context) : base(context) { }
    }
}
