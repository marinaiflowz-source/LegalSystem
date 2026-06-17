using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
   
    public class RefTaskTypeRepository : Repository<RefTaskType>
    {
        public RefTaskTypeRepository(ApplicationDbContext context) : base(context) { }
    }
}
