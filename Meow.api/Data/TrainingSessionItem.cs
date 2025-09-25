using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class TrainingSessionItem
{
    public Guid SessionItemID { get; set; }

    public Guid SessionID { get; set; }

    public Guid SetItemID { get; set; }

    public Guid VideoID { get; set; }

    public int OrderNo { get; set; }

    public string Status { get; set; } = null!;

    public int? ActualReps { get; set; }

    public decimal? ActualWeight { get; set; }

    public int? ActualDurationSec { get; set; }

    public int? ActualRestSec { get; set; }

    public int? RoundsDone { get; set; }

    public string? Note { get; set; }

    public virtual TrainingSession Session { get; set; } = null!;

    public virtual TrainingSetItem SetItem { get; set; } = null!;

    public virtual TrainingVideo Video { get; set; } = null!;
}
