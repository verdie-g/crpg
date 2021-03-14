using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;

namespace Crpg.Application.Strategus.Commands
{
    public class UpdateStrategusUserPositionsCommand : IMediatorRequest
    {
        public TimeSpan DeltaTime { get; set; }

        internal class Handler : IMediatorRequestHandler<UpdateStrategusUserPositionsCommand>
        {
            public async Task<Result> Handle(UpdateStrategusUserPositionsCommand req, CancellationToken cancellationToken)
            {
                return Result.NoErrors;
            }
        }
    }
}
