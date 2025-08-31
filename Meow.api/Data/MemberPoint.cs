using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Data;

[Table("MemberPoint")]
public partial class MemberPoint
{
    [Key]
    public Guid MemberID { get; set; }

    public int Balance { get; set; }

    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("MemberID")]
    [InverseProperty("MemberPoint")]
    public virtual Member Member { get; set; } = null!;
}
