using Crpg.Application.Common.Results;
using MediatR;

namespace Crpg.Application.Common.Mediator
{
    internal interface IMediatorRequest : IRequest<Result>
    {
    }

    internal interface IMediatorRequest<TResponse> : IRequest<Result<TResponse>> where TResponse : class
    {
    }
}
