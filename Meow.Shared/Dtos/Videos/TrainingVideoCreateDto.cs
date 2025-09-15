using System;
using System.Collections.Generic;

namespace Meow.Shared.Dtos.Videos
{
    public record TrainingVideoCreateDto(
        string Title,
        string? BodyPart,
        string Url,
        int? DurationSec,
        string? Status,
        List<Guid>? TagIds
    );
}
