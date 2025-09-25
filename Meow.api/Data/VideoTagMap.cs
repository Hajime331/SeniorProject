using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class VideoTagMap
{
    public Guid VideoID { get; set; }

    public Guid TagID { get; set; }

    public DateTime MappedAt { get; set; }

    public virtual Tag Tag { get; set; } = null!;

    public virtual TrainingVideo Video { get; set; } = null!;
}
