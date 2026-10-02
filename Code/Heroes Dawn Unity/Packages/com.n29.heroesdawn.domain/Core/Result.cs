#nullable enable
using System;

namespace N29.HeroesDawn.Core
{
    public readonly struct Result
    {
        public DomainError? Error { get; }
        public bool IsSuccess => Error == null;
        public bool IsFailure => Error != null;
        Result(DomainError? error) { Error = error; }
        public static Result Ok() => new Result(null);
        public static Result Fail(DomainError error) => new Result(error ?? throw new ArgumentNullException(nameof(error)));
    }

    public readonly struct Result<T>
    {
        readonly T _value;
        public DomainError? Error { get; }
        public bool IsSuccess => Error == null;
        public bool IsFailure => Error != null;
        public T Value => IsSuccess ? _value : throw new InvalidOperationException("Result en échec : " + Error);
        Result(T value, DomainError? error) { _value = value; Error = error; }
        public static Result<T> Ok(T value) => new Result<T>(value, null);
        public static Result<T> Fail(DomainError error) => new Result<T>(default!, error);
        public static implicit operator Result(Result<T> r) => r.IsSuccess ? Result.Ok() : Result.Fail(r.Error!);
    }
}
