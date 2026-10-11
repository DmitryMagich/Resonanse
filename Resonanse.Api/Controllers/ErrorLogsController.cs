using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resonanse.Application.Dtos;
using Resonanse.Application.Services;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/admin/logs")]
[Authorize(Roles = "Admin")]
public class ErrorLogsController : ControllerBase
{
    private readonly IErrorLogService _service;

    public ErrorLogsController(IErrorLogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ErrorLogDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? level = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] Guid? userId = null,
        CancellationToken ct = default)
    {
        var query = new ErrorLogQuery
        {
            Page = page,
            PageSize = pageSize,
            Level = level,
            Search = search,
            From = from,
            To = to,
            UserId = userId
        };

        return Ok(await _service.ListAsync(query, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ErrorLogDto>> GetById(Guid id, CancellationToken ct)
    {
        var log = await _service.GetByIdAsync(id, ct);
        return log is null ? NotFound() : Ok(log);
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(
        [FromQuery] DateTime? before,
        CancellationToken ct)
    {
        var deleted = await _service.ClearAsync(before, ct);
        return Ok(new { deleted });
    }
}