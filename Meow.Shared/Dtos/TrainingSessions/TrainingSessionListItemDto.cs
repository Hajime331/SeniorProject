using System;
using System.Collections.Generic;

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
    )
    {
        // 清單顯示用的標籤名稱（徽章）
        public List<string>? TagNames { get; init; }
    }
}
