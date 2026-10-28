using Resonanse.Domain.Enums;

namespace Resonanse.Application.Services;

public static class TrackSourceFilter
{
    public static TrackFileSource[] AllowedSources(ResonanceMode mode) => mode switch
    {
        ResonanceMode.Offline => new[]
        {
            TrackFileSource.Local
        },
        ResonanceMode.Online => new[]
        {
            TrackFileSource.Local,
            TrackFileSource.YouTube,
            TrackFileSource.ArchiveOrg
        },
        ResonanceMode.Resonance => new[]
        {
            TrackFileSource.Local,
            TrackFileSource.Peer,
            TrackFileSource.YouTube,
            TrackFileSource.ArchiveOrg,
            TrackFileSource.Other
        },
        _ => new[] { TrackFileSource.Local }
    };
}