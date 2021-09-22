using System;

namespace DirectoryWatchDog.FunctionslTools
{
    public static class FunctionalExtensions
    {
        public static T Pipe<T>(this T input, Func<T, T> func) => func(input);

        public static U Map<T, U>(this T input, Func<T, U> func) => func(input);

        /// <summary>
        /// Maps the Result<Error, T1> to Result<Error, T2>.
        /// </summary>
        /// <typeparam name="Error">The type of the Error.</typeparam>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <param name="input">The input Result<Error, T1></param>
        /// <param name="func">The transformer function.</param>
        public static Result<Error, T2> MapResult<Error, T1, T2>(this Result<Error, T1> input, Func<Result<Error, T1>, Result<Error, T2>> func) =>
            input.Sucess ? func(input) : input.ErrorValue.Failure<Error, T2>();

        public static Result<Error, T> Tee<Error, T>(this Result<Error, T> input, Action<T> action)
        {
            if (input.Sucess && action != null) action(input.Value);
            return input;
        }

        public static Result<Error, T> Match<Error, T>(this Result<Error, T> input,
            Action<Result<Error, T>> onFailure,
            Action<Result<Error, T>> onSuccess)
        {
            if (input.Sucess)
            {
                onSuccess(input);
            }
            else
            {
                onFailure(input);
            }

            return input;
        }

        public static Result<Error, T> Bind<Error, T>(this Result<Error, T> result, Func<T, Result<Error, T>> func) =>
            result.Sucess ? func(result.Value) : result;

        public static Result<Error, TResult> BindTo<Error, T, TResult>(this Result<Error, T> result, Func<T, Result<Error, TResult>> func) =>
            result.Sucess ? func(result.Value) : Result<Error, TResult>.Failure(result.ErrorValue);

        public static Result<Error, T> Ok<Error, T>(this T value) => Result<Error, T>.Ok(value);

        public static Result<Error, T> Failure<Error, T>(this Error error) => Result<Error, T>.Failure(error);
    }

    public class Result<Error, T>
    {
        public bool Sucess { get; set; }
        public T Value { get; set; }
        public Error ErrorValue { get; set; }

        private Result() { }

        public override string ToString()
        {
            return $"{{ Success: {Sucess}, Value: {Value}, ErrorValue: {ErrorValue} }}";
        }

        public static Result<Error, T> Ok(T value) => new Result<Error, T> { Sucess = true, Value = value };

        public static Result<Error, T> Failure(Error error) => new Result<Error, T> { Sucess = false, ErrorValue = error };

        public static Result<Error, T> Of(bool success, T value, Error error) =>
            new Result<Error, T> { Sucess = success, Value = success ? value : default(T), ErrorValue = success ? default(Error) : error };
    }
}