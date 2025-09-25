using System;
using System.Collections.Generic;

namespace Meow.Api.Data;

public partial class CatEvent
{
    public Guid EventID { get; set; }

    public string Code { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CatEventLog> CatEventLogs { get; set; } = new List<CatEventLog>();
}
