using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class UserRepository : Repository<TblUser>
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }
    }
}
