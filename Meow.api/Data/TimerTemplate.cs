using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class TimerTemplate
{
    public Guid TemplateID { get; set; }

    public Guid MemberID { get; set; }

    public string Name { get; set; } = null!;

    public int WorkSec { get; set; }

    public int RestSec { get; set; }

    public int Cycles { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Member Member { get; set; } = null!;
}
