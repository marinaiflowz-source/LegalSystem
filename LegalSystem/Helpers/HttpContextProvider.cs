using LegalSystem.DataAccess;
using LegalSystem.DTOs;
using LegalSystem.Enums;
using LegalSystem.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace LegalSystem.Helpers
{
    public class HttpContextProvider
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IWebHostEnvironment environment;
        private readonly UnitOfWork unitOfWork;

        public HttpContextProvider(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment, UnitOfWork unitOfWork)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.environment = environment;
            this.unitOfWork = unitOfWork;
        }

        internal async Task<CurrentUser> GetCurrentUser()
        {
            //if (environment.IsDevelopment())
            //{
            //    return GetInternalUser();
            //}
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext is null)
                throw new CustomException(FailResponseStatus.Unauthorized, "Not authorized");

            var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
            {
                throw new CustomException(FailResponseStatus.Unauthorized, "Not authorized");
            }
            if (!authHeader.StartsWith("Bearer ") && !httpContext.Request.Path.StartsWithSegments("/chat-hub"))
            {
                throw new CustomException(FailResponseStatus.Unauthorized, "Not authorized");
            }

            var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length).Trim() : authHeader;
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
                throw new CustomException(FailResponseStatus.Unauthorized, "Not authorized");

            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Payload;

            var acceptLanguage = httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString();
            var firstLanguage = acceptLanguage?.Split(',').FirstOrDefault();
            if (string.IsNullOrEmpty(firstLanguage) || firstLanguage != "ar")
                firstLanguage = "en";

            var result = new CurrentUser()
            {
                JwtToken = token,
                Language = firstLanguage,
                RefId = claims.TryGetValue("userId", out var userId) && userId != null ? Convert.ToInt32(userId) : 0,
                UserTypeId = claims.TryGetValue("userTypeId", out var userTypeId) && userTypeId != null ? Convert.ToInt32(userTypeId) : 0,
                Email = claims.TryGetValue("email", out var email) ? email.ToString() ?? string.Empty : string.Empty,
                UserName = claims.TryGetValue("email", out var userName) ? userName.ToString() ?? string.Empty : string.Empty,
                Mobile = claims.TryGetValue("mobile", out var mobile) ? mobile.ToString() ?? string.Empty : string.Empty,
                NameEn = claims.TryGetValue("nameEn", out var nameEn) ? nameEn.ToString() ?? string.Empty : string.Empty,
                NameAr = claims.TryGetValue("nameAr", out var nameAr) ? nameAr.ToString() ?? string.Empty : string.Empty,
                CompanyId = claims.TryGetValue("companyId", out var companyId) ? companyId.ToString() ?? string.Empty : string.Empty,
                AppProfileIds = claims.TryGetValue("AppProfileIds", out var profiles)
                    ? JsonSerializer.Deserialize<List<int>>(profiles.ToString() ?? JsonSerializer.Serialize<List<int>>(new List<int>())) ?? new List<int>()
                    : new List<int>()
            };
            result.IsSuperAdmin = result.UserTypeId == 1;
            var uid = await unitOfWork.UserRepository.GetAllQuerable().Where(e => e.RefId == result.RefId).Select(e => e.Id).FirstOrDefaultAsync();

            if (uid < 1)
                throw new CustomException(FailResponseStatus.NotFound, "Not linked into the IDM");

            result.UserId = uid;
            return result;
        }
        internal CurrentUser GetInternalUser()
        {
            var res = new CurrentUser()
            {
                UserId = 0,
                NameEn = "Public User",
                Email = "PublicUser",
            };

            return res;
        }
    }
}
