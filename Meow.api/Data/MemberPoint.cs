using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class MemberPoint
{
    public Guid MemberID { get; set; }

    public int Balance { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Member Member { get; set; } = null!;
}
