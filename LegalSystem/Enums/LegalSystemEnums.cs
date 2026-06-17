namespace LegalSystem.Enums
{
    public enum FailResponseStatus
    {
        Unauthorized = 1,
        Forbidden,
        BadRequest,
        NotFound,
        Failed,
        RequestTimeout,
        Conflict,
        UnsupportedMediaType,
        Locked,
        UpgradeRequired,
        TooManyRequests,
        RequestHeaderFieldsTooLarge,
        InternalServerError
    }
    public enum LanguageEnum { ar = 1, en = 2, fr = 3, zh = 4, hi = 5, ur = 6, ru = 7 }
    public enum ResponseEnum
    {
        Processing = StatusCodes.Status102Processing,
        Succeeded = StatusCodes.Status200OK,
        Created = StatusCodes.Status201Created,

        Unauthorized = StatusCodes.Status401Unauthorized,
        Forbidden = StatusCodes.Status403Forbidden,
        BadRequest = StatusCodes.Status400BadRequest,
        NotFound = StatusCodes.Status404NotFound,
        Conflict = StatusCodes.Status409Conflict,
        Failed = StatusCodes.Status412PreconditionFailed,

    }

    public enum ReasonTypeEnum
    {
        CASE = 1,
        TASK = 2,
        
        
    }
}
