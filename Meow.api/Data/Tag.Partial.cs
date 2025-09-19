using System.ComponentModel.DataAnnotations;

namespace Meow.Api.Data
{
    public partial class Tag
    {
        [StringLength(20)]
        public string Category { get; set; } = "一般"; // 對齊 DB: NVARCHAR(20) NOT NULL DEFAULT N'一般'
    }
}
