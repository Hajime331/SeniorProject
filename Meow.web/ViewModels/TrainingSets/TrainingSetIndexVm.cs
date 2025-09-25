using System;
using System.Collections.Generic;
using Meow.Shared.Dtos.Tags;
using Meow.Shared.Dtos.TrainingSets;

namespace Meow.Web.ViewModels.TrainingSets
{
    public class TrainingSetIndexVm
    {
        // 查詢條件
        public string? Keyword { get; set; }
        public string? Difficulty { get; set; }
        public Guid? TagId { get; set; }

        // 清單
        public IReadOnlyList<TrainingSetListItemDto> Sets { get; set; } = new List<TrainingSetListItemDto>();

        // 篩選選項
        public IReadOnlyList<TagDto> AllTags { get; set; } = new List<TagDto>();
        public IReadOnlyList<string> AllDifficulties { get; set; } = new List<string>();
    }
}
