using System;
using System.Collections.Generic;
using Meow.Shared.Dtos.Tags;

namespace Meow.Web.ViewModels.TrainingVideos
{
    public class TrainingVideoCreateVm
    {
        public string Title { get; set; } = "";
        public string BodyPart { get; set; } = "";
        public string Url { get; set; } = "";
        public int DurationSec { get; set; }
        public string Status { get; set; } = "Draft"; // 預設草稿
        public List<Guid> SelectedTagIds { get; set; } = new();
        public IReadOnlyList<TagDto> AllTags { get; set; } = Array.Empty<TagDto>();
        public string? ThumbnailUrl { get; set; }
    }

    public class TrainingVideoDetailVm
    {
        public Meow.Shared.Dtos.Videos.TrainingVideoDetailDto Video { get; set; } = default!;
        public IReadOnlyList<TagDto> AllTags { get; set; } = Array.Empty<TagDto>();
    }
}