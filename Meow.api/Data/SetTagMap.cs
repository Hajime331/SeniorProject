using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class SetTagMap
{
    public Guid SetID { get; set; }

    public Guid TagID { get; set; }

    public DateTime MappedAt { get; set; }

    public virtual TrainingSet Set { get; set; } = null!;

    public virtual Tag Tag { get; set; } = null!;
}
