using System;
using System.Collections.Generic;
using Meow.Shared.Dtos.Tags;
using Meow.Shared.Dtos.Videos;

namespace Meow.Web.ViewModels.TrainingSets
{
    public class TrainingSetCreateVm
    {
        public string Name { get; set; } = "";
        public string BodyPart { get; set; } = "全身";
        public string Equipment { get; set; } = "無器材";
        public string? Difficulty { get; set; }
        public int? EstimatedDurationSec { get; set; }
        public List<Guid> SelectedTagIds { get; set; } = new();
        public IReadOnlyList<TagDto> AllTags { get; set; } = Array.Empty<TagDto>();
        public IReadOnlyList<TrainingVideoListItemDto> AllVideos { get; set; } = Array.Empty<TrainingVideoListItemDto>();
        public List<TrainingSetItemInputVm> Items { get; set; } = new List<TrainingSetItemInputVm>();

        public IFormFile? CoverFile { get; set; }   // 上傳封面圖用
        public string? CoverUrl { get; set; }       // 已有封面圖的 URL（Edit 用）
    }

    public class TrainingSetItemInputVm
    {
        public Guid VideoId { get; set; }
        public int? OrderNo { get; set; }
        public int? TargetReps { get; set; }
        public int? RestSec { get; set; }
        public int? Rounds { get; set; }
    }

}