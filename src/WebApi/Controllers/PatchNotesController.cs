using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[AllowAnonymous]
[Route("patch-notes")]
public class PatchNotesController : BaseController
{
    private readonly IPatchNotesService _patchNotesService;

    public PatchNotesController(IPatchNotesService patchNotesService)
    {
        _patchNotesService = patchNotesService;
    }

    [HttpGet]
    public async Task<ActionResult<Result<IList<PatchNotes>>>> Get()
    {
        var patchNotes = await _patchNotesService.GetPatchNotesAsync(CancellationToken.None);
        return ResultToAction(new Result<IList<PatchNotes>>(patchNotes));
    }
}
