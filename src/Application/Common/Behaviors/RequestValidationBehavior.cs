using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using FluentValidation;
using MediatR;
using ValidationException = Crpg.Application.Common.Exceptions.ValidationException;

namespace Crpg.Application.Common.Behaviors
{
    internal class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IValidator<TRequest>[] _validators;

        public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = (validators as IValidator<TRequest>[])!;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (_validators.Length == 0)
            {
                return next();
            }

            var context = new ValidationContext<TRequest>(request);

            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures.Select(f => new ValidationError(f.ErrorMessage, f.PropertyName)));
            }

            return next();
        }
    }
}
