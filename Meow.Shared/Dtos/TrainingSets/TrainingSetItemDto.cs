using System;

namespace Meow.Shared.Dtos.TrainingSets
{
    public record TrainingSetItemDto(
        Guid SetItemID,
        Guid VideoID,
        int OrderNo,
        int? TargetReps,
        int? RestSec,
        int? Rounds
    );
}
