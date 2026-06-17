using LegalSystem.DataAccess;
using LegalSystem.DTOs;
using Microsoft.Extensions.Options;

namespace LegalSystem.Services
{
    public class CrmServiceOld
    {
        private readonly DapperDbContext dapperDbContext;
        private readonly ConnectionStrings connectionStrings;

        public CrmServiceOld(DapperDbContext dapperDbContext, IOptionsSnapshot<ConnectionStrings> connectionStringsOptions)
        {
            this.dapperDbContext = dapperDbContext;
            connectionStrings = connectionStringsOptions.Value;
        }
        public async Task<IEnumerable<CrmOrderQuery>> GetCrmOrdersAsync(CrmOrderFilter model)
        {
            var result = await dapperDbContext.ExecuteQueryWithListAsync<CrmOrderQuery>(connectionStrings.CRM, @"
            SELECT 
                ordr.ID RefId, 

                TRIM(cus.FullNameEnglish) CustomerNameEn,
                TRIM(cus.FullNameArabic) CustomerNameAr,
                TRIM(cus.Email) CustomerEmail,
                TRIM(cus.MobileNumber) CustomerMobile,

                TRIM(prj.Name) ProjectName,
                TRIM(prj.Code) ProjectCode,
                TRIM(unt.Number) UnitNumber,

                ordr.SoldPrice,

                ordr.Status StatusId,

                CASE ordr.Status
                    WHEN 2 THEN 'Booked'
                    WHEN 3 THEN 'Submitted'
                    WHEN 4 THEN 'Approved'
                    WHEN 5 THEN 'Declined'
                    WHEN 6 THEN 'Cancelled'
                    WHEN 8 THEN 'Sold'
                    WHEN 9 THEN 'Contract'
                    ELSE 'Unknown'
                END AS StatusName,

                ordr.CreatedDateTime CreatedDate

            FROM tblLead ordr
            INNER JOIN tblProject prj 
                ON prj.ID = ordr.ProjectID

            INNER JOIN tblUnit unt 
                ON unt.ID = ordr.UnitID

            INNER JOIN tblLeadCustomers ordrCus 
                ON ordrCus.LeadID = ordr.ID

            INNER JOIN tblCustomer cus 
                ON cus.ID = ordrCus.CustomerID

            WHERE 
                ordrCus.CustomerType = 1

                -- Search filter
                AND (
                    @Search IS NULL 
                    OR LTRIM(RTRIM(@Search)) = ''

                    OR TRIM(cus.FullNameEnglish) LIKE '%' + @Search + '%'
                    OR TRIM(cus.FullNameArabic) LIKE '%' + @Search + '%'
                    OR CAST(ordr.ID AS NVARCHAR) LIKE '%' + @Search + '%'
                    OR TRIM(cus.Email) LIKE '%' + @Search + '%'
                    OR TRIM(cus.MobileNumber) LIKE '%' + @Search + '%'
                    OR TRIM(prj.Name) LIKE '%' + @Search + '%'
                    OR TRIM(prj.Code) LIKE '%' + @Search + '%'
                    OR TRIM(unt.Number) LIKE '%' + @Search + '%'
                    OR CAST(ordr.SoldPrice AS NVARCHAR) LIKE '%' + @Search + '%'

                    OR (
                        CASE ordr.Status
                            WHEN 2 THEN 'Booked'
                            WHEN 3 THEN 'Submitted'
                            WHEN 4 THEN 'Approved'
                            WHEN 5 THEN 'Declined'
                            WHEN 6 THEN 'Cancelled'
                            WHEN 8 THEN 'Sold'
                            WHEN 9 THEN 'Contract'
                            ELSE 'Unknown'
                        END
                    ) LIKE '%' + @Search + '%'
                )

                -- Date filters
                AND (
                    @DateFrom IS NULL
                    OR ordr.CreatedDateTime >= @DateFrom
                )

                AND (
                    @DateTo IS NULL
                    OR ordr.CreatedDateTime < DATEADD(DAY, 1, @DateTo)
                )

            ORDER BY ordr.CreatedDateTime DESC
            OFFSET (@PageNumber - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY;
           "
            , new
            {
                Search = model.SearchText,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo,
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
            });

            return result;
        }
        public async Task<int> GetCrmOrderCountAsync(CrmOrderFilter model)
        {
            var result = await dapperDbContext.ExecuteQueryWithFirstAsync<int>(connectionStrings.CRM, @"
            SELECT COUNT(*) TotalCount
            FROM tblLead ordr
            INNER JOIN tblProject prj 
                ON prj.ID = ordr.ProjectID
            INNER JOIN tblUnit unt 
                ON unt.ID = ordr.UnitID
            INNER JOIN tblLeadCustomers ordrCus 
                ON ordrCus.LeadID = ordr.ID
            INNER JOIN tblCustomer cus 
                ON cus.ID = ordrCus.CustomerID
            WHERE 
                ordrCus.CustomerType = 1
                AND (
                    @Search IS NULL 
                    OR LTRIM(RTRIM(@Search)) = ''

                    OR TRIM(cus.FullNameEnglish) LIKE '%' + @Search + '%'
                    OR TRIM(cus.FullNameArabic) LIKE '%' + @Search + '%'
                    OR CAST(ordr.ID AS NVARCHAR) LIKE '%' + @Search + '%'
                    OR TRIM(cus.Email) LIKE '%' + @Search + '%'
                    OR TRIM(cus.MobileNumber) LIKE '%' + @Search + '%'
                    OR TRIM(prj.Name) LIKE '%' + @Search + '%'
                    OR TRIM(prj.Code) LIKE '%' + @Search + '%'
                    OR TRIM(unt.Number) LIKE '%' + @Search + '%'
                    OR CAST(ordr.SoldPrice AS NVARCHAR) LIKE '%' + @Search + '%'

                    OR (
                        CASE ordr.Status
                            WHEN 2 THEN 'Booked'
                            WHEN 3 THEN 'Submitted'
                            WHEN 4 THEN 'Approved'
                            WHEN 5 THEN 'Declined'
                            WHEN 6 THEN 'Cancelled'
                            WHEN 8 THEN 'Sold'
                            WHEN 9 THEN 'Contract'
                            ELSE 'Unknown'
                        END
                    ) LIKE '%' + @Search + '%'
                )
	                -- Date filters
                AND (
                    @DateFrom IS NULL
                    OR ordr.CreatedDateTime >= @DateFrom
                )

                AND (
                    @DateTo IS NULL
                    OR ordr.CreatedDateTime < DATEADD(DAY, 1, @DateTo)
                )
           "
            , new
            {
                Search = model.SearchText,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo,
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
            });

            return result;
        }
        public async Task<IEnumerable<CrmOrderBuyerQuery>> GetCrmOrderBuyersAsync(long refId)
        {
            var result = await dapperDbContext.ExecuteQueryWithListAsync<CrmOrderBuyerQuery>(connectionStrings.CRM, @"
            SELECT 

            ordrCus.LeadID RefId,

            TRIM(cus.FullNameEnglish) CustomerNameEn,
            TRIM(cus.FullNameArabic) CustomerNameAr,
            TRIM(cus.Email) CustomerEmail,
            TRIM(cus.MobileNumber) CustomerMobile,

            ordrCus.CustomerType CustomerTypeId,
                CASE ordrCus.CustomerType
                    WHEN 1 THEN 'Main Buyer'
                    WHEN 2 THEN 'Joint Buyer'
                    ELSE 'Unknown'
                END AS CustomerType

            FROM tblLeadCustomers ordrCus
            INNER JOIN tblCustomer cus ON cus.ID = ordrCus.CustomerID

            WHERE ordrCus.LeadID = @RefId
           "
            , new
            {
                RefId = refId
            });

            return result;
        }
    }
}
