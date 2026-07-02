namespace Domain.ValueObjects;

public class HandlerRequestResult<T>
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public T SuccessValue { get; set; }
    public HandlerErrorType ErrorType { get; init; }

    public HandlerRequestResult()
    {
    }

    public HandlerRequestResult(string errorMessage, HandlerErrorType errorType = HandlerErrorType.None)
    {
        Success = false;
        ErrorMessage = errorMessage;
        ErrorType = errorType;
    }

    public HandlerRequestResult(T result)
    {
        Success = true;
        SuccessValue = result;
    }
}
