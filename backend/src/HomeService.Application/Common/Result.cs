namespace HomeService.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public string[] Errors { get; }

    protected Result(bool isSuccess, string message, string[] errors)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors;
    }

    public static Result Success(string message = "")
        => new(true, message, Array.Empty<string>());

    public static Result<T> Success<T>(T data, string message = "")
        => new(true, message, Array.Empty<string>(), data);

    public static Result Failure(string message, params string[] errors)
        => new(false, message, errors);

    public static Result<T> Failure<T>(string message, params string[] errors)
        => new(false, message, errors, default!);
}

public class Result<T> : Result
{
    public T Data { get; }

    internal Result(bool isSuccess, string message, string[] errors, T data)
        : base(isSuccess, message, errors)
    {
        Data = data;
    }
}
