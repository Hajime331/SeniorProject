using System;

namespace Meow.Shared.Dtos.Tags
{
    public class TagDto
    {
        public Guid TagId { get; set; }
        public string Name { get; set; } = default!;
        public string Category { get; set; } = default!; // '部位' / '一般'
    }
}