using System.Collections.Generic;

namespace Crpg.Application.Common.Results
{
    /// <summary>
    /// Generic result, inspired by JSON API (https://jsonapi.org) that should be returned by any request.
    /// </summary>
    public class Result
    {
        public static readonly Result NoErrors = new();

        /// <summary>
        /// A collection of error objects.
        /// </summary>
        public IList<Error>? Errors { get; }

        public Result()
        {
        }

        public Result(Error error) => Errors = new[] { error };

        public Result(IList<Error> errors) => Errors = errors;
    }

    /// <inheritdoc />
    public class Result<TData> : Result where TData : class
    {
        /// <summary>
        /// The document’s primary data.
        /// </summary>
        public TData? Data { get; }

        public Result(TData data) => Data = data;

        public Result(Error error)
            : base(error)
        {
        }

        public Result(IList<Error> errors)
            : base(errors)
        {
        }
    }
}
