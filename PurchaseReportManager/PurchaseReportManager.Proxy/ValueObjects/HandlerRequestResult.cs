namespace PurchaseReportManager.Proxy;

public sealed class HandlerRequestResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;

    public static HandlerRequestResult<T> Ok(T data) =>
        new() { IsSuccess = true, Data = data };

    public static HandlerRequestResult<T> Fail(string error) =>
        new() { IsSuccess = false, ErrorMessage = error };
}
