using Crpg.Application.Common.Results;
using MediatR;

namespace Crpg.Application.Common.Mediator
{
    internal interface IMediatorRequestHandler<in TRequest> : IRequestHandler<TRequest, Result>
        where TRequest : IRequest<Result>
    {
    }

    internal interface IMediatorRequestHandler<in TRequest, TResponse> : IRequestHandler<TRequest, Result<TResponse>>
        where TRequest : IRequest<Result<TResponse>>
        where TResponse : class
    {
    }
}
