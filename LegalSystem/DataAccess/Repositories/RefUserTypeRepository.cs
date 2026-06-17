using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class RefUserTypeRepository : Repository<RefUserType>
    {
        public RefUserTypeRepository(ApplicationDbContext context) : base(context) { }
    }
}
