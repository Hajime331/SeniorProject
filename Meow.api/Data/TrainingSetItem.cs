using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class TrainingSetItem
{
    public Guid SetItemID { get; set; }

    public Guid SetID { get; set; }

    public Guid VideoID { get; set; }

    public int OrderNo { get; set; }

    public int? TargetReps { get; set; }

    public int? RestSec { get; set; }

    public int? Rounds { get; set; }

    public virtual TrainingSet Set { get; set; } = null!;

    public virtual ICollection<TrainingSessionItem> TrainingSessionItems { get; set; } = new List<TrainingSessionItem>();

    public virtual TrainingVideo Video { get; set; } = null!;
}
