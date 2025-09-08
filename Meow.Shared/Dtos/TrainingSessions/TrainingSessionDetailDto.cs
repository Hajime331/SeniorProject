using System;
using System.Collections.Generic;

namespace Meow.Shared.Dtos.TrainingSessions
{
    public class TrainingSessionDetailDto
    {
        public Guid SessionID { get; set; }
        public Guid MemberID { get; set; }      // 之後可由 Claims 取代
        public Guid SetID { get; set; }
        public string SetName { get; set; } = default!;
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public bool CompletedFlag { get; set; }
        public string? Notes { get; set; }
        public int? CaloriesBurned { get; set; }
        public int? PointsAwarded { get; set; }

        public List<TrainingSessionItemDto> Items { get; set; } = new();
    }
}
