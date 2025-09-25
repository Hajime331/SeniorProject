using System;
using System.Collections.Generic;

namespace Meow.Shared.Dtos.TrainingSets
{
    public record TrainingSetUpdateDto(
        Guid SetId,
        string Name,
        string BodyPart,
        string Equipment,
        string? Difficulty,
        int? EstimatedDurationSec,
        List<Guid> TagIds,
        List<TrainingSetItemUpdateDto> Items,
        string? CoverUrl
    );
}