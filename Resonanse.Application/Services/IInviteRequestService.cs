using Resonanse.Application.Dtos;

namespace Resonanse.Application.Services;

public interface IInviteRequestService
{
    Task<InviteRequestDto> CreateAsync(CreateInviteRequestBody body, CancellationToken ct = default);
    Task<List<InviteRequestDto>> ListAsync(string? status, CancellationToken ct = default);
    Task<InviteRequestDto?> ApproveAsync(Guid requestId, Guid adminUserId, CancellationToken ct = default);
    Task<InviteRequestDto?> RejectAsync(Guid requestId, Guid adminUserId, CancellationToken ct = default);
}