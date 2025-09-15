using System;
using System.Collections.Generic;

namespace Meow.Shared.Dtos.TrainingSets
{
    public record TrainingSetCreateDto(
        string Name,
        string? BodyPart,
        string? Equipment,
        string? Difficulty,
        int? EstimatedDurationSec,
        bool IsCustom,
        Guid? OwnerMemberID,
        string? Status,
        List<Guid>? TagIds,
        List<TrainingSetItemCreateDto> Items
    );
}
