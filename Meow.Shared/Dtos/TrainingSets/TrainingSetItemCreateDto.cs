using System;

namespace Meow.Shared.Dtos.TrainingSets
{
    public record TrainingSetItemCreateDto(
        Guid VideoId,        // ← 命名一致
        int? OrderNo,
        int? TargetReps,
        int? RestSec,
        int? Rounds
    );
}