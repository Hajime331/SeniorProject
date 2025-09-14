using Meow.Shared.Dtos.Analytics;
using Meow.Shared.Dtos.TrainingSessions;

namespace Meow.Web.ViewModels.Dashboard
{
    public sealed class MyWeeklyVm
    {
        public MemberWeeklySummaryDto Weekly { get; init; } = new();
        public List<TrainingSessionListItemDto> Recent { get; init; } = new();
        public MemberStatsDto? Stats { get; init; }
    }
}
