using LegalSystem.Enums;
using System.Net;

namespace LegalSystem.Exceptions
{
    public class CustomException : Exception
    {
        /// <summary>
        /// Status Code
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// Message to bre previewed to the user
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Status Code Title or URL
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// More Details
        /// </summary>
        public string? Details { get; set; }

        public CustomException(FailResponseStatus failResponseStatus, string message, string? details = null)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            switch (failResponseStatus)
            {
                case FailResponseStatus.Unauthorized: statusCode = HttpStatusCode.Unauthorized; break;
                case FailResponseStatus.Forbidden: statusCode = HttpStatusCode.Forbidden; break;
                case FailResponseStatus.BadRequest: statusCode = HttpStatusCode.BadRequest; break;
                case FailResponseStatus.NotFound: statusCode = HttpStatusCode.NotFound; break;
                case FailResponseStatus.Conflict: statusCode = HttpStatusCode.Conflict; break;
                case FailResponseStatus.RequestTimeout: statusCode = HttpStatusCode.RequestTimeout; break;
                case FailResponseStatus.UnsupportedMediaType: statusCode = HttpStatusCode.UnsupportedMediaType; break;
                case FailResponseStatus.Locked: statusCode = HttpStatusCode.Locked; break;
                case FailResponseStatus.UpgradeRequired: statusCode = HttpStatusCode.UpgradeRequired; break;
                case FailResponseStatus.TooManyRequests: statusCode = HttpStatusCode.TooManyRequests; break;
                case FailResponseStatus.RequestHeaderFieldsTooLarge: statusCode = HttpStatusCode.RequestHeaderFieldsTooLarge; break;
                case FailResponseStatus.InternalServerError: statusCode = HttpStatusCode.InternalServerError; break;

                case FailResponseStatus.Failed: break;
            }

            Status = (int)statusCode;
            Type = statusCode.ToString();
            Title = message;
            Details = details;
        }
    }
}
