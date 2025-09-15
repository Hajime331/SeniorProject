using System;
using System.Collections.Generic;

namespace Meow.Shared.Dtos.TrainingSets
{
    public class TrainingSetDetailDto
    {
        public Guid SetID { get; set; }
        public string Name { get; set; } = default!;
        public string BodyPart { get; set; } = default!;
        public string Equipment { get; set; } = default!;
        public string? Difficulty { get; set; }
        public int? EstimatedDurationSec { get; set; }
        public bool IsCustom { get; set; }
        public Guid? OwnerMemberID { get; set; }
        public string Status { get; set; } = default!;
        public List<Guid> TagIds { get; set; } = new();
        public List<TrainingSetItemDto> Items { get; set; } = new();
    }
}
