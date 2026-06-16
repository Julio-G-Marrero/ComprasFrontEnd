namespace Domain.Dtos;

internal sealed class ApiResponseDto<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
}
