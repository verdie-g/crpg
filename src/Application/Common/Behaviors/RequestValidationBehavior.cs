using System.Diagnostics;
using System.Runtime.CompilerServices;
using Crpg.Application.Common.Results;
using FluentValidation;
using MediatR;

namespace Crpg.Application.Common.Behaviors;

internal class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    static RequestValidationBehavior()
    {
        Debug.Assert(typeof(Result).IsAssignableFrom(typeof(TResponse)),
            $"Request {typeof(TRequest).Name} should return a {nameof(Result)} type");
    }

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

        ValidationContext<TRequest> context = new(request);
        List<Error> errors = new();
        foreach (var validator in _validators)
        {
            foreach (var failure in validator.Validate(context).Errors)
            {
                errors.Add(new Error(ErrorType.Validation, ErrorCode.InvalidField)
                {
                    Title = "Invalid field",
                    Detail = failure.ErrorMessage,
                    Source = new ErrorSource { Parameter = failure.PropertyName },
                });
            }
        }

        return errors.Count != 0
            ? Task.FromResult(Unsafe.As<TResponse>(new Result(errors)))
            : next();
    }
}
