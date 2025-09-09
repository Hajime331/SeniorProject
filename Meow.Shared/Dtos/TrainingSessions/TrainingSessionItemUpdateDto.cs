using System;
using System.ComponentModel.DataAnnotations;

namespace Meow.Shared.Dtos.TrainingSessions
{
    public class TrainingSessionItemUpdateDto
    {
        [Required]
        public Guid SessionItemID { get; set; }

        [Required]
        public Guid SessionID { get; set; }

        public string? Status { get; set; }          // Done/Skipped/Partial
        public int? ActualReps { get; set; }
        public decimal? ActualWeight { get; set; }
        public int? ActualDurationSec { get; set; }
        public int? ActualRestSec { get; set; }
        public int? RoundsDone { get; set; }
        public string? Note { get; set; }
    }
}
