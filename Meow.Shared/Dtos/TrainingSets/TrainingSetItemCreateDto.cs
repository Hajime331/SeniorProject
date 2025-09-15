using System;

namespace Meow.Shared.Dtos.TrainingSets
{
    public record TrainingSetItemCreateDto(
        Guid VideoID,
        int? OrderNo,
        int? TargetReps,
        int? RestSec,
        int? Rounds
    );
}
