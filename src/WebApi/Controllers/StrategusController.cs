using System.Collections.Generic;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Models;
using Crpg.Application.Strategus.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers
{
    [Authorize(Policy = UserPolicy)]
    public class StrategusController : BaseController
    {
        /// <summary>
        /// Get strategus settlements.
        /// </summary>
        [HttpGet("settlements")]
        public Task<ActionResult<Result<IList<StrategusSettlementViewModel>>>> GetUser()
            => ResultToActionAsync(Mediator.Send(new GetStrategusSettlementsQuery()));
    }
}
