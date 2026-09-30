using Resonanse.Application.Dtos.Auth;

namespace Resonanse.Application.Services;

public interface IInviteService
{
    Task<InviteDto> CreateAsync(Guid adminUserId, int validDays, CancellationToken ct = default);
    Task<List<InviteDto>> ListAsync(CancellationToken ct = default);
    Task<bool> RevokeAsync(Guid inviteId, CancellationToken ct = default);
} // thx AI