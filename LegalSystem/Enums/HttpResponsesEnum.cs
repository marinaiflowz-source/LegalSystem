namespace LegalSystem.Enums
{
    public enum HttpResponsesEnum
    {
        Succeeded = 200,
        Created = 201,
        Accepted = 202,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        RequestTimeout = 408,
        Conflict = 409,
        UnsupportedMediaType = 415,
        Locked = 423,
        UpgradeRequired = 426,
        TooManyRequests = 429,
        RequestHeaderFieldsTooLarge = 431,
        AuthenticationTimeout = 419,
        InternalServerError = 500
    }
}
