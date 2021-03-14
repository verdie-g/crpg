using Crpg.Application.Common.Results;
using MediatR;

namespace Crpg.Application.Common.Mediator
{
    public interface IMediatorRequest : IRequest<Result>
    {
    }

    public interface IMediatorRequest<TResponse> : IRequest<Result<TResponse>> where TResponse : class
    {
    }
}
