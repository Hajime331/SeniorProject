using Meow.Shared.Dtos.TrainingSessions;

namespace Meow.Web.ViewModels.TrainingSessions
{
    public class TrainingSessionListVm
    {
        public IReadOnlyList<TrainingSessionListItemDto> Items { get; init; } = [];
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }

        // 篩選條件（回填到表單）
        public DateTime? From { get; init; }
        public DateTime? To { get; init; }
    }
}
