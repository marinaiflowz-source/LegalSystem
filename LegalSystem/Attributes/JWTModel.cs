namespace LegalSystem.Attributes
{
    public record JWTModel
    {
        public string Secret { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public int AppProfileId { get; set; }
    }
}
