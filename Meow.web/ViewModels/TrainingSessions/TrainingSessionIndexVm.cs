using Meow.Shared.Dtos.TrainingSessions;
using Meow.Shared.Dtos.Common;
using System;
using System.Collections.Generic;

public class TrainingSessionIndexVm
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    // 用字串承接：可以是 Guid 字串，也可以是「胸部」「拉伸」
    public List<string> SelectedTagIds { get; set; } = new();

    // (TagID, Name, Category) — 若你暫時沒 Category 就放 null
    public List<(Guid TagID, string Name, string? Category)> AllTags { get; set; } = new();

    public PagedResultDto<TrainingSessionListItemDto>? Result { get; set; }
}
