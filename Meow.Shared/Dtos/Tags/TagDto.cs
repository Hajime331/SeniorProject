using System;

namespace Meow.Shared.Dtos.Tags
{
    public class TagDto
    {
        public Guid TagId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; } // '部位' / '一般'
    }
}
