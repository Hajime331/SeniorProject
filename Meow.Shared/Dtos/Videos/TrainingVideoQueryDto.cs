using System;
using Meow.Shared.Dtos.Common;

namespace Meow.Shared.Dtos.Videos
{
    public class TrainingVideoQueryDto : PagingQuery
    {
        public string? Status { get; set; }
        public string? BodyPart { get; set; }
        public Guid? TagId { get; set; }
    }
}