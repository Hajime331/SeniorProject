using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("TimerTemplate")]
public partial class TimerTemplate
{
    [Key]
    public Guid TemplateID { get; set; }

    public Guid MemberID { get; set; }

    [StringLength(80)]
    public string Name { get; set; } = null!;

    public int WorkSec { get; set; }

    public int RestSec { get; set; }

    public int Cycles { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("MemberID")]
    [InverseProperty("TimerTemplates")]
    public virtual Member Member { get; set; } = null!;
}
