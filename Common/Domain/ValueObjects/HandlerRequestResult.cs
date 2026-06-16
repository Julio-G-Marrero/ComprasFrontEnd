namespace Domain.ValueObjects;

public class HandlerRequestResult<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string ErrorMessage { get; init; }
    public string ErrorType { get; init; }

    public static HandlerRequestResult<T> SuccessResult(T data)
        => new() { Success = true, Data = data, ErrorMessage = string.Empty, ErrorType = string.Empty };

    public static HandlerRequestResult<T> ErrorResult(string message, string type = "Error")
        => new() { Success = false, Data = default, ErrorMessage = message, ErrorType = type };
}

public class HandlerRequestResult
{
    public bool Success { get; init; }
    public string ErrorMessage { get; init; }
    public string ErrorType { get; init; }

    public static HandlerRequestResult SuccessResult()
        => new() { Success = true, ErrorMessage = string.Empty, ErrorType = string.Empty };

    public static HandlerRequestResult ErrorResult(string message, string type = "Error")
        => new() { Success = false, ErrorMessage = message, ErrorType = type };
}
