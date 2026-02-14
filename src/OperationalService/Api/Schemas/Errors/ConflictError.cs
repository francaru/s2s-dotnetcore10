namespace OperationalService.Api.Schemas.Errors;

public sealed class ConflictError
{
    public required string ErrorCode { get; set; }

    public required string Message { get; set; }
}
