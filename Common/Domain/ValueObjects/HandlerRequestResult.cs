namespace Domain.ValueObjects;

public class HandlerRequestResult
{
    public bool Success { get; init; }
    public string ErrorMessage { get; init; }
    public HandlerErrorType ErrorType { get; init; }

    public HandlerRequestResult()
    {
        Success = true;
        ErrorMessage = string.Empty;
        ErrorType = HandlerErrorType.None;
    }

    public HandlerRequestResult(string message, HandlerErrorType type = HandlerErrorType.None)
    {
        Success = false;
        ErrorMessage = message;
        ErrorType = type;
    }
}
