using LegalSystem.DataAccess.Repositories.Base;
using LegalSystem.Models;

namespace LegalSystem.DataAccess.Repositories
{
    public class RefDocumentClassificationRepository : Repository<RefDocumentClassification>
    {
        public RefDocumentClassificationRepository(ApplicationDbContext context) : base(context) { }
    }
}
