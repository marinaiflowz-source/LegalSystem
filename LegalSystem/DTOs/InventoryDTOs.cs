namespace LegalSystem.DTOs
{
    public class OrderFilter : IQueryObject
    {
        public string? SortBy { get; set; }
        public bool IsAscending { get; set; }
        public int Index { get; set; }
        public int Size { get; set; }

        public string? SearchText { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class CrmOrderFilter
    {
        public string? SearchText { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
    }

    public class CrmOrderQuery
    {
        public long RefId { get; set; }

        public string CustomerNameEn { get; set; } = string.Empty;
        public string CustomerNameAr { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerMobile { get; set; } = string.Empty;

        public string ProjectName { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;

        public float SoldPrice { get; set; }

        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
    public class CrmOrderBuyerQuery
    {
        public long RefId { get; set; }

        public string CustomerNameEn { get; set; } = string.Empty;
        public string CustomerNameAr { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerMobile { get; set; } = string.Empty;
        public int CustomerTypeId { get; set; }
        public string CustomerType { get; set; } = string.Empty;
    }
}
