namespace Crpg.Application.Common.Results
{
    public class Error
    {
        /// <summary>
        /// The correlation ID provided by the gateway.
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// A machine-readable code specifying error category. This information is used on client side to focus on
        /// certain type of error, to either retry some processing or display only certain type of errors.
        /// </summary>
        public ErrorType Type { get; }

        /// <summary>
        /// A unique machine-readable error code string.
        /// </summary>
        public ErrorCode Code { get; }

        /// <summary>
        /// A short, human-readable summary of the problem type. It should not change from occurrence to occurrence
        /// of the problem, except for purposes of localization.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// A human-readable explanation specific to this occurrence of the problem.
        /// </summary>
        public string? Detail { get; set; }

        /// <summary>
        /// A machine-readable structure to reference to the exact location causing the error.
        /// </summary>
        public ErrorSource? Source { get; set; }

        /// <summary>
        /// A human-readable stacktrace.
        /// </summary>
        public string? StackTrace { get; set; }

        public Error(ErrorType type, ErrorCode code)
        {
            Type = type;
            Code = code;
        }

        public override string ToString() => Detail ?? Title ?? Code.ToString();
    }
}
