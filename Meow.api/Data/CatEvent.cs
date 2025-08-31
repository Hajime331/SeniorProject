using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("CatEvent")]
[Index("Code", Name = "UQ_CatEvent_Code", IsUnique = true)]
public partial class CatEvent
{
    [Key]
    public Guid EventID { get; set; }

    [StringLength(60)]
    public string Code { get; set; } = null!;

    [StringLength(120)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("Event")]
    public virtual ICollection<CatEventLog> CatEventLogs { get; set; } = new List<CatEventLog>();
}
