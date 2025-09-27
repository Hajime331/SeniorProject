using System;
using System.Collections.Generic;

namespace Meow.Shared.Dtos.Videos
{
    public record TrainingVideoUpdateDto(
        Guid VideoId,
        string Title,
        string BodyPart,
        string Url,
        int DurationSec,
        string Status,
        List<Guid> TagIds,
        string? ThumbnailUrl
    );
}