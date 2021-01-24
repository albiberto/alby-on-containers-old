using System.Collections.Generic;
using System.Linq;

namespace IdentityServer
{
    public interface IResult<out TValue, out TError>
    {
        TValue Success { get; }
        IEnumerable<TError> Errors { get; }
    }

    public class Result<TValue, TError> : IResult<TValue, TError>
    {
        public Result(TValue values, IEnumerable<TError> errors)
        {
            Success = values;
            Errors = errors;
        }

        public TValue Success { get; }
        public IEnumerable<TError> Errors { get; }
    }

    public static class Result<T>
    {
        public static IResult<TValue, T> Value<TValue>(TValue value) => new Result<TValue, T>(value, Enumerable.Empty<T>());

        public static IResult<T, TError> Errors<TError>(IEnumerable<TError> errors) => new Result<T, TError>(default, errors);

        public static IResult<T, TError> Error<TError>(TError error) => new Result<T, TError>(default, new[] {error});
    }

    public static class Result
    {
        public static bool HasErrors<TValue, TError>(this IResult<TValue, TError> source) => source?.Errors?.Any() ?? false;
    }
}