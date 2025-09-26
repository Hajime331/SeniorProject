using System;
using System.Collections.Generic;
using Meow.Shared.Dtos.TrainingSets;

namespace Meow.Web.ViewModels.TrainingSets
{
    public class TrainingSetDetailVm
    {
        public Guid SetId { get; set; }
        public string Name { get; set; } = "";
        public string BodyPart { get; set; } = "";
        public string Equipment { get; set; } = "";
        public string? Difficulty { get; set; }
        public int? EstimatedDurationSec { get; set; }
        public string Status { get; set; } = "";
        public string? CoverUrl { get; set; }

        public IReadOnlyList<Guid> TagIds { get; set; } = new List<Guid>();
        public List<TrainingSetItemDetailVm> Items { get; set; } = new();


    }
}
