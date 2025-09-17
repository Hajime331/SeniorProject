using System;
using System.ComponentModel.DataAnnotations;

namespace Meow.Shared.Dtos.TrainingSets
{
    public record TrainingSetItemCreateDto(
    Guid VideoId,     // ← 改命名
    int? OrderNo,
    int? TargetReps,
    int? RestSec,
    int? Rounds
    );

}
