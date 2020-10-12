using System.Collections.Generic;

namespace Crpg.Application.Common.Results
{
    /// <summary>
    /// Generic result, inspired by JSON API (https://jsonapi.org) that should be returned by any request.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// A collection of error objects.
        /// </summary>
        public IList<Error>? Errors { get; }

        protected Result() { }
        protected Result(Error error) => Errors = new[] { error };
        protected Result(IList<Error> errors) => Errors = errors;
    }

    /// <typeparam name="TData">Tye of the data. Use <see cref="object"/> if there is no data.</typeparam>
    public class Result<TData> : Result where TData : class
    {
        /// <summary>
        /// The document’s primary data.
        /// </summary>
        public TData? Data { get; }

        public Result() { }
        public Result(TData data) => Data = data;
        public Result(Error error) : base(error) { }
        public Result(IList<Error> errors) : base(errors) { }
    }
}
