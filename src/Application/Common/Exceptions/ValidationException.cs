using System;
using System.Collections.Generic;

namespace Crpg.Application.Common.Exceptions
{
    public class ValidationException : Exception
    {
        public IEnumerable<ValidationError> Errors { get; }

        public ValidationException(IEnumerable<ValidationError> errors) => Errors = errors;
    }

    public class ValidationError
    {
        public string ErrorMessage { get; }
        public string PropertyName { get; }

        public ValidationError(string errorMessage, string propertyName)
        {
            ErrorMessage = errorMessage;
            PropertyName = propertyName;
        }
    }
}
