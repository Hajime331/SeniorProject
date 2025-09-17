using System;

namespace Meow.Shared.Dtos.TrainingSets
{
    public record TrainingSetItemDto(
        Guid SetItemId,
        Guid VideoId,
        int OrderNo,
        int? TargetReps,
        int? RestSec,
        int? Rounds
    );
}
