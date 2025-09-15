using System;
using System.Collections.Generic;

namespace Meow.Shared.Dtos.TrainingSets
{
    public record TrainingSetListItemDto(
        Guid SetID,
        string Name,
        string BodyPart,
        string Equipment,
        string? Difficulty,
        int? EstimatedDurationSec,
        List<Guid> TagIds,
        int ItemCount
    );
}
