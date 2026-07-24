using LegalSystem.Enums;
using System.Data;

namespace LegalSystem.DTOs
{
    public interface IQueryObject
    {
        string? SortBy { get; set; }
        bool IsAscending { get; set; }
        int Index { get; set; }
        int Size { get; set; }
    }
    public class Response
    {
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; } = (int)ResponseEnum.Succeeded;

        public Response()
        {
        }

        public Response(ResponseEnum responseStatus, string title)
        {
            Status = (int)responseStatus;
            Title = title;
        }
    }
    public class Response<T> : Response
    {
        public T? Data { get; set; }

        public Response() { }

        public Response(T data)
        {
            Data = data;
        }

        public Response(T data, string title)
        {
            Data = data;
            Title = title;
        }

        public Response(T data, ResponseEnum responseStatus)
        {
            Data = data;
            Status = (int)responseStatus;
        }

        public Response(ResponseEnum responseStatus, string title)
        {
            Status = (int)responseStatus;
            Title = title;
        }

        public Response(T data, ResponseEnum responseStatus, string title)
        {
            Data = data;
            Status = (int)responseStatus;
            Title = title;
        }

    }

    public class IdentityResponse
    {
        public HttpResponsesEnum Status { get; set; }

        public string Message { get; set; } = string.Empty;

        public IdentityResponse()
        {
        }

        public IdentityResponse(HttpResponsesEnum status, string message)
        {
            Status = status;
            Message = message;
        }

        public IdentityResponse(HttpResponsesEnum status)
        {
            Status = status;
        }
    }
    public class IdentityResponse<T> : IdentityResponse
    {
        public T Data { get; set; }

    }
    public class JWTResponseModel
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class QueryResult<T>
    {
        public QueryResult()
        {
            Data = new QueryResultData<T>();
        }

        public QueryResult(string title) : this()
        {
            Title = title;
        }

        public QueryResult(QueryResultData<T> data)
        {
            Data = data;
        }

        public QueryResult(QueryResultData<T> data, string title)
        {
            Data = data;
            Title = title;
        }

        public string Title { get; set; } = string.Empty;
        public int Status { get; set; } = (int)ResponseEnum.Succeeded;
        public QueryResultData<T> Data { get; set; }
    }
    public class QueryResultData<T>
    {
        public int Count { get; set; }
        public IEnumerable<T> Rows { get; set; } = new List<T>();
    }
    public class CurrentUser
    {
        public string JwtToken { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public int RefId { get; set; }
        public long UserId { get; set; }
        public int UserTypeId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public List<int> AppProfileIds { get; set; } = new List<int>();
        public bool IsSuperAdmin { get; set; }
    }
    public class SummaryView
    {
        public long? Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class SummaryUserView
    {
        public long? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long? UserId { get; set; }
    }
    public class UserSummaryView
    {
        public long Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Type { get; set; }
    }
    public class DownloadFileView
    {
        public MemoryStream Stream { get; set; } = new();
        public string Name { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
    }

    public class ExecuteQueryModel
    {
        /// <summary>
        /// CRM, PACT-4, PACT-32, PACT-7, PACT-25
        /// </summary>
        public required string ConnectionString { get; set; }

        /// <summary>
        /// Stored procedure name, sql function name... etc
        /// </summary>
        public required string DestinationName { get; set; }

        /// <summary>
        /// Text, TableDirect, StoredProcedure
        /// </summary>
        public required CommandType CommandType { get; set; }

        /// <summary>
        /// parameters passed to the DestinationName
        /// </summary>
        public object? Parameters { get; set; } = null;
    }
}
