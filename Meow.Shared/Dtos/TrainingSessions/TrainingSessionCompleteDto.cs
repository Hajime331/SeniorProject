using System;

namespace Meow.Shared.Dtos.TrainingSessions
{
    public class TrainingSessionCompleteDto
    {
        public DateTime EndedAt { get; set; } = DateTime.UtcNow;
        public bool CompletedFlag { get; set; } = true;
        public int? CaloriesBurned { get; set; }
        public int? PointsAwarded { get; set; }
        public string? Notes { get; set; }
    }
}
