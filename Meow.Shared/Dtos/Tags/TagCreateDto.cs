using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meow.Shared.Dtos.Tags
{
    public record TagCreateDto(string Name, string Category); // Category：'部位' / '一般'
}
