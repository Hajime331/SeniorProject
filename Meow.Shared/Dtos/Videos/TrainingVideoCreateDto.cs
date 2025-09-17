// Meow.Shared/Dtos/Videos/TrainingVideoCreateDto.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Meow.Shared.Dtos.Videos
{
    public record TrainingVideoCreateDto(
        string Title,
        string BodyPart,      // 必填
        string Url,
        int DurationSec,      // 必填；後端驗證 >0
        string Status,        // 必填；後端白名單驗證
        List<Guid> TagIds     // 非 null
    );

}
