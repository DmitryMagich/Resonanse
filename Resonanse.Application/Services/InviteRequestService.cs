using Microsoft.EntityFrameworkCore;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Dtos;
using Resonanse.Domain.Entities;
using Resonanse.Domain.Enums;

namespace Resonanse.Application.Services;

public class InviteRequestService : IInviteRequestService
{
    private readonly IResonanseDbContext _db;

    public InviteRequestService(IResonanseDbContext db)
    {
        _db = db;
    }

    public async Task<InviteRequestDto> CreateAsync(CreateInviteRequestBody body, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(body.Name) || body.Name.Trim().Length < 2)
            throw new InvalidOperationException("Name must be at least 2 characters.");

        var entity = new InviteRequest
        {
            Name = body.Name.Trim(),
            Comment = string.IsNullOrWhiteSpace(body.Comment) ? null : body.Comment.Trim(),
            Contact = string.IsNullOrWhiteSpace(body.Contact) ? null : body.Contact.Trim(),
            Status = InviteRequestStatus.Pending
        };

        _db.InviteRequests.Add(entity);
        await _db.SaveChangesAsync(ct);

        return Map(entity, null);
    }

    public async Task<List<InviteRequestDto>> ListAsync(string? status, CancellationToken ct = default)
    {
        var q = _db.InviteRequests.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<InviteRequestStatus>(status, true, out var parsed))
        {
            q = q.Where(r => r.Status == parsed);
        }

        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                Request = r,
                ProcessedByUsername = r.ProcessedBy != null ? r.ProcessedBy.Username : null
            })
            .ToListAsync(ct);

        return items.Select(x => Map(x.Request, x.ProcessedByUsername)).ToList();
    }

    public async Task<InviteRequestDto?> ApproveAsync(Guid requestId, Guid adminUserId, CancellationToken ct = default)
    {
        var request = await _db.InviteRequests
            .Include(r => r.ProcessedBy)
            .FirstOrDefaultAsync(r => r.Id == requestId, ct);

        if (request is null) return null;
        if (request.Status != InviteRequestStatus.Pending)
            throw new InvalidOperationException("Request is already processed.");

        request.Status = InviteRequestStatus.Approved;
        request.ProcessedAt = DateTime.UtcNow;
        request.ProcessedByUserId = adminUserId;
        request.UpdatedAt = DateTime.UtcNow;

        // Автоматически создаём инвайт
        var invite = new Invite
        {
            Code = GenerateInviteCode(),
            CreatedByUserId = adminUserId,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        _db.Invites.Add(invite);

        await _db.SaveChangesAsync(ct);

        // Достаём ProcessedBy для DTO
        var processedBy = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == adminUserId, ct);

        return Map(request, processedBy?.Username);
    }

    public async Task<InviteRequestDto?> RejectAsync(Guid requestId, Guid adminUserId, CancellationToken ct = default)
    {
        var request = await _db.InviteRequests
            .Include(r => r.ProcessedBy)
            .FirstOrDefaultAsync(r => r.Id == requestId, ct);

        if (request is null) return null;
        if (request.Status != InviteRequestStatus.Pending)
            throw new InvalidOperationException("Request is already processed.");

        request.Status = InviteRequestStatus.Rejected;
        request.ProcessedAt = DateTime.UtcNow;
        request.ProcessedByUserId = adminUserId;
        request.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        var processedBy = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == adminUserId, ct);

        return Map(request, processedBy?.Username);
    }

    private static InviteRequestDto Map(InviteRequest r, string? processedByUsername) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Comment = r.Comment,
        Contact = r.Contact,
        Status = r.Status,
        CreatedAt = r.CreatedAt,
        ProcessedAt = r.ProcessedAt,
        ProcessedByUserId = r.ProcessedByUserId,
        ProcessedByUsername = processedByUsername
    };

    private static string GenerateInviteCode()
    {
        var bytes = Guid.NewGuid().ToByteArray();
        var hex = Convert.ToHexString(bytes).ToLowerInvariant();
        return $"{hex[..4]}-{hex[4..8]}-{hex[8..12]}-{hex[12..16]}";
    }
}