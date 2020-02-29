using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Crpg.Application.Common.Behaviours
{
    public class RequestPerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private static readonly TimeSpan Threshold = TimeSpan.FromMilliseconds(500);

        private readonly Stopwatch _timer;
        private readonly ILogger<TRequest> _logger;
        private readonly ICurrentUserService _currentUserService;

        public RequestPerformanceBehaviour(ILogger<TRequest> logger, ICurrentUserService currentUserService)
        {
            _timer = new Stopwatch();

            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                _timer.Start();
                return await next();
            }
            finally
            {
                _timer.Stop();

                if (_timer.ElapsedMilliseconds > Threshold.TotalMilliseconds)
                {
                    _logger.LogWarning("Long Running Request: {RequestName} {ElapsedMilliseconds} ms ({@UserId})",
                        typeof(TRequest).Name, _timer.ElapsedMilliseconds, _currentUserService.UserId);
                }
            }
        }
    }
}