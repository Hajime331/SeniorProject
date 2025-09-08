using System;
using System.ComponentModel.DataAnnotations;

namespace Meow.Shared.Dtos.TrainingSessions
{
    public class TrainingSessionCreateDto
    {
        [Required]
        public Guid SetID { get; set; }

        public string? Notes { get; set; }
    }
}
