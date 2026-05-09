namespace SaaSPlatform.SharedKernel.Results;

public class Result
{
    public bool Succeeded { get; init; }
    public string Message { get; init; } = string.Empty;
    public List<string> Errors { get; init; } = new();

    public static Result Success(string message = "")
        => new() { Succeeded = true, Message = message };

    public static Result Failure(string message, List<string>? errors = null)
        => new()
        {
            Succeeded = false,
            Message = message,
            Errors = errors ?? new()
        };
}

public class Result<T> : Result
{
    public T? Data { get; init; }

    public static Result<T> Success(T data, string message = "")
        => new()
        {
            Succeeded = true,
            Data = data,
            Message = message
        };

    public new static Result<T> Failure(string message, List<string>? errors = null)
        => new()
        {
            Succeeded = false,
            Message = message,
            Errors = errors ?? new()
        };
}