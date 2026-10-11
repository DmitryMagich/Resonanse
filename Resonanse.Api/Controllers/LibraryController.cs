using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Dtos;
using Resonanse.Application.Services;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/library")]
[Authorize(Roles = "Admin")]
public class LibraryController : ControllerBase
{
    private readonly IScanJobService _scanJobs;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<LibraryController> _logger;

    public LibraryController(
        IScanJobService scanJobs,
        ICurrentUser currentUser,
        ILogger<LibraryController> logger)
    {
        _scanJobs = scanJobs;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Enqueues a background scan job. Returns immediately with a job id.
    /// </summary>
    [HttpPost("scan")]
    public async Task<ActionResult<ScanJobDto>> Scan(
        [FromQuery] string? path,
        CancellationToken ct)
    {
        try
        {
            var job = await _scanJobs.EnqueueAsync(path, trigger: "manual", _currentUser.UserId, ct);
            _logger.LogInformation("Manual scan enqueued: jobId={JobId}, path={Path}", job.Id, job.Path);
            return Accepted(job);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Returns the status of a specific scan job.
    /// </summary>
    [HttpGet("scan/status/{jobId:guid}")]
    public async Task<ActionResult<ScanJobDto>> GetStatus(Guid jobId, CancellationToken ct)
    {
        var job = await _scanJobs.GetByIdAsync(jobId, ct);
        return job is null ? NotFound() : Ok(job);
    }

    /// <summary>
    /// Returns the most recent scan job (any status).
    /// </summary>
    [HttpGet("scan/latest")]
    public async Task<ActionResult<ScanJobDto>> GetLatest(CancellationToken ct)
    {
        var job = await _scanJobs.GetLatestAsync(ct);
        return job is null ? NotFound() : Ok(job);
    }

    /// <summary>
    /// Returns scan history, newest first.
    /// </summary>
    [HttpGet("scan/history")]
    public async Task<ActionResult<List<ScanJobDto>>> GetHistory(
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        return Ok(await _scanJobs.GetHistoryAsync(limit, ct));
    }
}