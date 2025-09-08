using System.Collections.Generic;

namespace Meow.Shared.Dtos.Common
{
    public class PagedResultDto<T>
    {
        public IReadOnlyList<T> Items { get; init; } = [];
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
    }
}