using System.Collections.Generic;
using Meow.Shared.Dtos.TrainingSets;

namespace Meow.Web.ViewModels.TrainingSets
{
    public class TrainingSetIndexVm
    {
        public string? Keyword { get; set; }
        public IReadOnlyList<TrainingSetListItemDto> Sets { get; set; } = new List<TrainingSetListItemDto>();
    }
}
