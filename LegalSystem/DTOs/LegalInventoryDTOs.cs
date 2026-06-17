namespace LegalSystem.DTOs
{
    public class LegalInventoryDto
    {
       public string  LeadID {  get; set; }
       public string ProjectID {  get; set; }
       public string ProjectCode {  get; set; }
       public string ProjectName { get; set; }
       public string UnitNumber { get; set; }
       public string UnitType { get; set; }
       public string LeadStatus { get; set; }
       public string BuyerName { get; set; }
       public string BuyerNumber { get; set; }
       public string JointBuyerName { get; set; }
       public string JointBuyerMobile { get; set; }
       public string SoldPrice { get; set; }

    }

    public class InventoryFilter
    {
       
        public int Index { get; set; } = 0;   
        public int Size { get; set; } = 10;  
        public string? SearchText { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectName { get; set; }
        public string? UnitType { get; set; }
        public string? LeadStatus { get; set; }
        public string? UnitNumber { get; set; }
        
        public string? LeadID { get; set; }
        public string? BuyerNumber { get; set; }
        public string Sort { get; set; } = "LeadID";
        public bool IsAscending { get; set; } = false;
    }

   public class ProjectLookupDto
    {
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }

    }
}
