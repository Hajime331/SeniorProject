using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meow.Shared.Dtos.Analytics;

public class PopularTrainingSetDto
{
    public Guid SetID { get; set; }
    public string SetName { get; set; } = "";
    public int CompletedCount { get; set; }
    public int TotalMinutes { get; set; }
    public double AvgMinutes => CompletedCount == 0 ? 0 : (double)TotalMinutes / CompletedCount;
    public DateTime? LastCompletedAtUtc { get; set; }
}

