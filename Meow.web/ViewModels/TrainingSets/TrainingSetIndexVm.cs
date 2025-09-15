using System.Collections.Generic;
using Meow.Shared.Dtos.TrainingSets;

namespace Meow.Web.ViewModels.TrainingSets
{
    public class TrainingSetIndexVm
    {
        public string? Keyword { get; set; }
        public IReadOnlyList<TrainingSetListDto> Sets { get; set; } = new List<TrainingSetListDto>();
    }
}
