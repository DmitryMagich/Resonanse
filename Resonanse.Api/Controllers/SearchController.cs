using Microsoft.AspNetCore.Mvc;
using Resonanse.Application.Dtos;
using Resonanse.Application.Services;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly ILibraryQueryService _query;

    public SearchController(ILibraryQueryService query)
    {
        _query = query;
    }

    [HttpGet]
    public async Task<ActionResult<SearchResultDto>> Search(
        [FromQuery] string q,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        return Ok(await _query.SearchAsync(q, limit, ct));
    }
}