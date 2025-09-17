using System;
using System.Collections.Generic;
using Meow.Shared.Dtos.Tags;
using Meow.Shared.Dtos.Videos;

namespace Meow.Web.ViewModels.TrainingVideos
{
    public class TrainingVideoIndexVm
    {
        public string? Keyword { get; set; }
        // Draft / Published / Archived；留空表示全部
        public string? Status { get; set; }

        // 篩選：多選標籤（Guid）
        public List<Guid> SelectedTagIds { get; set; } = new();

        // 供 checkbox 顯示
        public IReadOnlyList<TagDto> AllTags { get; set; } = Array.Empty<TagDto>();

        // 清單結果
        public IReadOnlyList<TrainingVideoListItemDto> Videos { get; set; } = Array.Empty<TrainingVideoListItemDto>();
    }
}
