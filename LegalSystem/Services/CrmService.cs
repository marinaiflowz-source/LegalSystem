using Dapper;
using LegalSystem.DTOs;
using Microsoft.Data.SqlClient;

namespace LegalSystem.Services
{
    public class CrmService
    {
        private readonly IConfiguration _configuration;

        public CrmService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IQueryable<LegalInventoryDto> GetInventoryQueryable()
        {
            var result = new List<LegalInventoryDto>();

            var connection = new SqlConnection(_configuration.GetConnectionString("CRM"));
            connection.Open();

            var query = @"
            SELECT 
            LeadID,
            SoldPrice,
            ProjectID,
            ProjectCode,
            ProjectName,
            UnitNumber,
            UnitType,
            LeadStatus,
            BuyerName,
            BuyerNumber,
            JointBuyerName,
            JointBuyerMobile
            FROM vw_CRM_Legal_Invontory";

            return connection.Query<LegalInventoryDto>(query).AsQueryable();
        }

    }
}
