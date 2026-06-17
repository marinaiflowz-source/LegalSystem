using LegalSystem.DTOs;
using LegalSystem.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace LegalSystem.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequiredPermissionAttribute : Attribute, IAsyncActionFilter, IFilterMetadata
    {
        private readonly string[] _claimTypes = Array.Empty<string>();

        public RequiredPermissionAttribute(params string[] ClaimTypes)
        {
            _claimTypes = ClaimTypes;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            HttpContext httpContext = context.HttpContext;
            IConfiguration _configuration = httpContext.RequestServices.GetService<IConfiguration>();
            string token = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (token == null)
            {
                context.Result = new ObjectResult(new
                {
                    Message = "User does not have the required permissions. User token is empty!"
                })
                {
                    StatusCode = 403
                };
                return;
            }

            int appProfileId = _configuration.GetValue<int>("AppProfileId");
            if (appProfileId < 1)
            {
                context.Result = new ObjectResult(new
                {
                    Message = "AppProfileId does not exist."
                })
                {
                    StatusCode = 403
                };
                return;
            }

            string authApiUrl = _configuration.GetValue<string>("AuthApiUrl");
            if (string.IsNullOrEmpty(authApiUrl))
            {
                context.Result = new ObjectResult(new
                {
                    Message = "Authentication Url does not exist."
                })
                {
                    StatusCode = 403
                };
                return;
            }

            IServiceProvider serviceProvider = httpContext.RequestServices;
            if (!(serviceProvider.GetService(typeof(IHttpClientFactory)) is IHttpClientFactory httpClientFactory))
            {
                context.Result = new ObjectResult(new
                {
                    Message = "HttpClient could not be instantiated."
                })
                {
                    StatusCode = 403
                };
                return;
            }

            HttpClient client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", token);
            HttpResponseMessage response = await client.GetAsync(authApiUrl + "/Authentication/GetMyStatus");
            if (!response.IsSuccessStatusCode)
            {
                context.Result = new ObjectResult(new IdentityResponse
                {
                    Status = HttpResponsesEnum.Unauthorized,
                    Message = "Could not get user status"
                })
                {
                    StatusCode = (int)response.StatusCode
                };
                return;
            }

            if (int.TryParse(await response.Content.ReadAsStringAsync(), out var numeric))
            {
                switch ((USER_STATUS)numeric)
                {
                    case USER_STATUS.IN_ACTIVE:
                        context.Result = new ObjectResult(new
                        {
                            Status = 404,
                            Message = "User does not exist."
                        })
                        {
                            StatusCode = 401
                        };
                        return;
                    case USER_STATUS.LOCKED:
                        context.Result = new ObjectResult(new
                        {
                            Message = "User is locked."
                        })
                        {
                            StatusCode = 423
                        };
                        return;
                }
            }

            if (_claimTypes.Length == 0)
            {
                TokenValidator tokenValidator = new TokenValidator(_configuration);
                SecurityToken validatedToken;
                ClaimsPrincipal validateTokenResult = tokenValidator.ValidateToken(token, out validatedToken);
                if (validateTokenResult == null)
                {
                    context.Result = new ObjectResult(new
                    {
                        Message = "Invalid Token!"
                    })
                    {
                        StatusCode = 401
                    };
                    return;
                }
            }
            else
            {
                var requestBody = new
                {
                    AppProfileId = appProfileId,
                    ClaimTypes = _claimTypes
                };
                StringContent jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                client = httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", token);
                response = await client.PostAsync(authApiUrl + "/Authentication/UserHasRight", jsonContent);
                bool flag = !response.IsSuccessStatusCode;
                bool flag2 = flag;
                if (!flag2)
                {
                    flag2 = !(await response.Content.ReadAsStringAsync()).Contains("true");
                }

                if (flag2)
                {
                    context.Result = new ObjectResult(new
                    {
                        Message = "User has no permissions."
                    })
                    {
                        StatusCode = 403
                    };
                    return;
                }
            }

            await next();
        }
    }
}
