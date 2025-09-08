using System;

namespace Meow.Shared.Dtos.TrainingSessions
{
    public class TrainingSessionItemDto
    {
        public Guid SessionItemID { get; set; }
        public Guid SetItemID { get; set; }
        public Guid VideoID { get; set; }
        public int OrderNo { get; set; }
        public string Status { get; set; } = "Done"; // Done/Skipped/Partial
        public int? ActualReps { get; set; }
        public decimal? ActualWeight { get; set; }
        public int? ActualDurationSec { get; set; }
        public int? ActualRestSec { get; set; }
        public int? RoundsDone { get; set; }
        public string? Note { get; set; }

        // 方便前端顯示（非必填欄）
        public string VideoTitle { get; set; } = default!;
    }
}
