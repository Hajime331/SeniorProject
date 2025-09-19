using System;
using System.Collections.Generic;

namespace Meow.Shared.Dtos.Videos
{
    public record TrainingVideoCreateDto(
        string Title,
        string BodyPart,
        string Url,
        int DurationSec,   // 必填且 > 0（由後端驗證）
        string Status,        // 必填：Draft/Published/Archived（由後端白名單驗證）
        List<Guid> TagIds     // 非 null；空集合可
    );
}