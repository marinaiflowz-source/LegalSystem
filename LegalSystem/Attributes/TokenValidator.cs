using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LegalSystem.Attributes
{
    public class TokenValidator
    {
        private readonly TokenValidationParameters _tokenValidationParameters;

        public TokenValidator(IConfiguration config)
        {
            _tokenValidationParameters = GetJwtTokenValidationParams(config);
        }

        public ClaimsPrincipal? ValidateToken(string token, out SecurityToken? validatedToken)
        {
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring("Bearer ".Length);
            }

            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            try
            {
                return jwtSecurityTokenHandler.ValidateToken(token, _tokenValidationParameters, out validatedToken);
            }
            catch (Exception)
            {
                validatedToken = null;
                return null;
            }
        }

        public TokenValidationParameters GetJwtTokenValidationParams(IConfiguration config)
        {
            JWTModel jWT = config.GetSection("JWT").Get<JWTModel>();
            return new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jWT.Secret)),
                ValidIssuer = jWT.Issuer,
                ValidAudience = jWT.Audience,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}
