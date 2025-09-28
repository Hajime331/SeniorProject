using System;
using System.Collections.Generic;

namespace Meow.Shared.Dtos.Common
{
    public class PagedResultDto<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int TotalCount { get; init; }
        public int Page { get; init; }              // 原本的 Page
        public int PageSize { get; init; }

        // 新增：給 Razor 用的別名
        public int PageIndex => Page;

        // 已有的總頁數
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        // （可選）再加兩個便利屬性
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}
