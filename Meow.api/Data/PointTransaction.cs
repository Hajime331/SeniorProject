using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class PointTransaction
{
    public Guid TxnID { get; set; }

    public Guid MemberID { get; set; }

    public string Direction { get; set; } = null!;

    public int Amount { get; set; }

    public string Source { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid? RefID { get; set; }

    public Guid? EventLogID { get; set; }

    public int? SignedAmount { get; set; }

    public virtual CatEventLog? EventLog { get; set; }

    public virtual Member Member { get; set; } = null!;
}
