using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class RefCompanyRepository : Repository<RefCompany>
    {
        public RefCompanyRepository(ApplicationDbContext context) : base(context) { }
    }
}
