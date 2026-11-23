using System.Security.Permissions;
using HotelListingApi.Constants;

namespace HotelListingApi.Results;

public readonly record struct Error(string Code, string Description)
{
    public static readonly Error None = new(ErrorCodes.NotFound, "The requested resource was not found.");
    public bool IsNone => string.IsNullOrWhiteSpace(Code);
}

public readonly record struct Result
{
    public bool IsSuccess {get;}
    public Error[] Errors {get;}
    private Result(bool isSuccess, Error[] errors)
        => (IsSuccess, Errors) = (isSuccess, errors);
    public static Result Success() => new(true, []);
    public static Result Failure(params Error[] errors) => new(false, errors);
    public static Result NotFound(params Error[] errors) => new(false, errors);
    public static Result BadRequest(params Error[] errors) => new(false, errors);
    public static Result Combine(params Result[] results)
    {
        return results.Any(r => !r.IsSuccess)
            ? Failure(results.Where(r => !r.IsSuccess).SelectMany(r => r.Errors).ToArray())
            : Success();
    }
}

public readonly record struct Result<T>
{
    public bool IsSuccess {get;}
    public T? Value {get;}
    public Error[] Errors {get;}
    private Result(bool isSuccess, T? value, Error[] errors)
        => (IsSuccess, this.Value, Errors) = (isSuccess, value, errors);
    public static Result<T> Success(T value) => new(true, value, Array.Empty<Error>());
    public static Result<T> Failure(params Error[] errors) => new(false, default, errors);
    public static Result<T> NotFound() => new(false, default, []);
    public static Result<T> BadRequest() => new(false, default, []);
    public static Result<T> BadRequest(params Error[] errors) => new(false, default, errors);

    // Function helpers

    public Result<K> Map<K>(Func<T, K> mapFunc)
        => IsSuccess ? Result<K>.Success(mapFunc(Value!)) : Result<K>.Failure(Errors);

    public Result<K> Bind<K>(Func<T, Result<K>> next)
        => IsSuccess ? next(Value!) : Result<K>.Failure(Errors);

    public Result<T> Ensure(Func<T, bool> predicate, Error error)
        => IsSuccess && !predicate(Value!) ? Failure(error) : this;
}
