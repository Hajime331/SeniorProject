using System;

namespace Meow.Shared.Dtos.TrainingSessions
{
    public record TrainingSessionListItemDto(
        Guid SessionID,
        DateTime StartedAt,
        DateTime? EndedAt,
        bool CompletedFlag,
        string SetName,
        string? Notes,
        int? CaloriesBurned,
        int? PointsAwarded
    );
}