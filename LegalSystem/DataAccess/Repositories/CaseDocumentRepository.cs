using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class CaseDocumentRepository : Repository<TblCaseDocument>
    {
        public CaseDocumentRepository(ApplicationDbContext context) : base(context) { }
    }
}
